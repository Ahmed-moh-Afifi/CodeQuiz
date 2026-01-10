namespace CodeQuizDesktop.Models;

/// <summary>
/// Model for quiz statistics calculations including grading curve data.
/// </summary>
public class QuizStatistics : BaseObservableModel
{
    private int _totalAttempts;
    public int TotalAttempts
    {
        get => _totalAttempts;
        set { _totalAttempts = value; OnPropertyChanged(); }
    }

    private int _submittedAttempts;
    public int SubmittedAttempts
    {
        get => _submittedAttempts;
        set { _submittedAttempts = value; OnPropertyChanged(); }
    }

    private int _pendingAttempts;
    public int PendingAttempts
    {
        get => _pendingAttempts;
        set { _pendingAttempts = value; OnPropertyChanged(); }
    }

    private double _averageGrade;
    public double AverageGrade
    {
        get => _averageGrade;
        set { _averageGrade = value; OnPropertyChanged(); }
    }

    private double _medianGrade;
    public double MedianGrade
    {
        get => _medianGrade;
        set { _medianGrade = value; OnPropertyChanged(); }
    }

    private double _highestGrade;
    public double HighestGrade
    {
        get => _highestGrade;
        set { _highestGrade = value; OnPropertyChanged(); }
    }

    private double _lowestGrade;
    public double LowestGrade
    {
        get => _lowestGrade;
        set { _lowestGrade = value; OnPropertyChanged(); }
    }

    private double _standardDeviation;
    public double StandardDeviation
    {
        get => _standardDeviation;
        set { _standardDeviation = value; OnPropertyChanged(); }
    }

    private double _passRate;
    /// <summary>
    /// Percentage of students who passed (grade >= 50% of total)
    /// </summary>
    public double PassRate
    {
        get => _passRate;
        set { _passRate = value; OnPropertyChanged(); }
    }

    private float _totalPoints;
    public float TotalPoints
    {
        get => _totalPoints;
        set { _totalPoints = value; OnPropertyChanged(); }
    }

    private List<GradeDistributionBucket> _gradeDistribution = [];
    /// <summary>
    /// Distribution of grades in buckets (e.g., 0-10, 10-20, etc.)
    /// </summary>
    public List<GradeDistributionBucket> GradeDistribution
    {
        get => _gradeDistribution;
        set { _gradeDistribution = value; OnPropertyChanged(); }
    }

    private List<double> _normalDistributionValues = [];
    /// <summary>
    /// Normal distribution curve values for comparison (as percentages)
    /// </summary>
    public List<double> NormalDistributionValues
    {
        get => _normalDistributionValues;
        set { _normalDistributionValues = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Calculates statistics from a list of attempts.
    /// </summary>
    public static QuizStatistics Calculate(IEnumerable<ExaminerAttempt> attempts, float totalPoints)
    {
        var attemptsList = attempts.ToList();
        var stats = new QuizStatistics
        {
            TotalAttempts = attemptsList.Count,
            TotalPoints = totalPoints
        };

        if (attemptsList.Count == 0)
        {
            return stats;
        }

        // Filter to submitted attempts with grades
        var gradedAttempts = attemptsList.Where(a => a.EndTime != null && a.Grade.HasValue).ToList();
        stats.SubmittedAttempts = gradedAttempts.Count;
        stats.PendingAttempts = attemptsList.Count - gradedAttempts.Count;

        if (gradedAttempts.Count == 0)
        {
            return stats;
        }

        var grades = gradedAttempts.Select(a => (double)a.Grade!.Value).OrderBy(g => g).ToList();

        // Basic statistics
        stats.AverageGrade = Math.Round(grades.Average(), 2);
        stats.HighestGrade = grades.Max();
        stats.LowestGrade = grades.Min();

        // Median
        int mid = grades.Count / 2;
        stats.MedianGrade = grades.Count % 2 == 0
            ? Math.Round((grades[mid - 1] + grades[mid]) / 2, 2)
            : grades[mid];

        // Standard Deviation
        if (grades.Count > 1)
        {
            double avg = grades.Average();
            double sumOfSquaresOfDifferences = grades.Sum(g => Math.Pow(g - avg, 2));
            // Use Sample Standard Deviation (N-1) instead of Population Standard Deviation (N)
            // This provides a better estimation of the normal curve for the class
            stats.StandardDeviation = Math.Round(Math.Sqrt(sumOfSquaresOfDifferences / (grades.Count - 1)), 2);
        }
        else
        {
            // For single grade, use a reasonable default standard deviation for visualization
            stats.StandardDeviation = totalPoints * 0.15; // 15% of total points
        }

        // Pass rate (>= 50% of total points)
        double passingThreshold = totalPoints * 0.5;
        int passedCount = grades.Count(g => g >= passingThreshold);
        stats.PassRate = Math.Round((double)passedCount / grades.Count * 100, 1);

        // Grade distribution (10 buckets based on percentage of total)
        stats.GradeDistribution = CalculateGradeDistribution(grades, totalPoints);
        stats.NormalDistributionValues = CalculateNormalDistribution(stats.AverageGrade, stats.StandardDeviation, totalPoints);

        return stats;
    }

    private static List<GradeDistributionBucket> CalculateGradeDistribution(List<double> grades, float totalPoints)
    {
        var buckets = new List<GradeDistributionBucket>();
        int bucketCount = 10;
        double bucketSize = totalPoints / bucketCount;

        for (int i = 0; i < bucketCount; i++)
        {
            double rangeStart = i * bucketSize;
            double rangeEnd = (i + 1) * bucketSize;
            
            int count = grades.Count(g => g >= rangeStart && (i == bucketCount - 1 ? g <= rangeEnd : g < rangeEnd));
            double percentage = grades.Count > 0 ? (double)count / grades.Count * 100 : 0;
            
            buckets.Add(new GradeDistributionBucket
            {
                RangeStart = rangeStart,
                RangeEnd = rangeEnd,
                Count = count,
                Percentage = Math.Round(percentage, 2),
                Label = $"{(int)(i * 10)}-{(int)((i + 1) * 10)}%"
            });
        }

        return buckets;
    }

    /// <summary>
    /// Calculates the ideal theoretical normal distribution values as percentages for each bucket.
    /// Uses fixed "ideal" statistics to create a perfect bell curve centered at 50% for comparison,
    /// regardless of actual student performance.
    /// </summary>
    private static List<double> CalculateNormalDistribution(double mean, double stdDev, float totalPoints)
    {
        var values = new List<double>();
        int bucketCount = 10;
        double bucketSize = totalPoints / bucketCount;

        // Use "ideal" statistics for a perfect theoretical bell curve:
        // - Ideal Mean: Centers the peak at 50% of total points
        // - Ideal StdDev: totalPoints/6 ensures ~99.7% of the curve fits within [0, totalPoints]
        //   (3 standard deviations on each side of the mean)
        double idealMean = totalPoints / 2.0;
        double idealStdDev = totalPoints / 6.0;

        for (int i = 0; i < bucketCount; i++)
        {
            double rangeStart = i * bucketSize;
            double rangeEnd = (i + 1) * bucketSize;
            
            // Calculate the probability mass for this bucket using CDF difference
            double zStart = (rangeStart - idealMean) / idealStdDev;
            double zEnd = (rangeEnd - idealMean) / idealStdDev;
            double probability = NormalCDF(zEnd) - NormalCDF(zStart);
            
            // Convert probability to percentage (probability * 100)
            double expectedPercentage = probability * 100;
            
            values.Add(Math.Max(0, Math.Round(expectedPercentage, 2)));
        }

        return values;
    }

    /// <summary>
    /// Approximation of the normal cumulative distribution function using the error function.
    /// </summary>
    private static double NormalCDF(double z)
    {
        // Using Abramowitz and Stegun approximation (maximum error: 7.5×10?8)
        const double a1 = 0.254829592;
        const double a2 = -0.284496736;
        const double a3 = 1.421413741;
        const double a4 = -1.453152027;
        const double a5 = 1.061405429;
        const double p = 0.3275911;

        // Save the sign of z
        int sign = z < 0 ? -1 : 1;
        z = Math.Abs(z) / Math.Sqrt(2);

        // A&S formula 7.1.26
        double t = 1.0 / (1.0 + p * z);
        double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-z * z);

        return 0.5 * (1.0 + sign * y);
    }
}

/// <summary>
/// Represents a bucket in the grade distribution histogram.
/// </summary>
public class GradeDistributionBucket
{
    public double RangeStart { get; set; }
    public double RangeEnd { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
    public string Label { get; set; } = "";
}
