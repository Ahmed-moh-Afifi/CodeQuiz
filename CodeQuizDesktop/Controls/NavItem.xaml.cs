namespace CodeQuizDesktop.Controls;

public partial class NavItem : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(NavItem), string.Empty);

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(NavItem), false, propertyChanged: OnIsSelectedChanged);

    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(NavItem), string.Empty);

    private Border? _borderElement;
    private bool _isPointerOver;

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private Color iconColor = Color.FromArgb("#aaaaaa");

    public Color IconColor
    {
        get { return iconColor; }
        set
        {
            iconColor = value;
            OnPropertyChanged();
        }
    }


    public NavItem()
    {
        InitializeComponent();

        this.Loaded += (s, e) =>
        {
            _borderElement = this.FindByName<Border>("borderElement");
            SetupGestureHandlers();
            UpdateBackgroundColor();
        };
    }

    private void SetupGestureHandlers()
    {
        if (_borderElement == null) return;

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            IsSelected = true;
        };
        _borderElement.GestureRecognizers.Add(tapGesture);

        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += (s, e) =>
        {
            _isPointerOver = true;
            UpdateBackgroundColor();
        };

        pointerGesture.PointerExited += (s, e) =>
        {
            _isPointerOver = false;
            UpdateBackgroundColor();
        };

        _borderElement.GestureRecognizers.Add(pointerGesture);
    }

    private static void OnIsSelectedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NavItem navItem)
        {
            navItem.UpdateBackgroundColor();

            // Update icon color based on selection state
            if ((bool)newValue)
            {
                navItem.IconColor = Color.FromArgb("#FFFFFFFF");
            }
            else
            {
                navItem.IconColor = Color.FromArgb("#aaaaaa");
            }
        }
    }

    private void UpdateBackgroundColor()
    {
        if (_borderElement == null) return;

        if (IsSelected)
        {

            _borderElement.BackgroundColor = Color.FromArgb("#591c21");

        }
        else if (_isPointerOver)
        {
            _borderElement.BackgroundColor = Color.FromArgb("#262626");
        }
        else
        {
            _borderElement.BackgroundColor = Colors.Transparent;
        }
    }
}