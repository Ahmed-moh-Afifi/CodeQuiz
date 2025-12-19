# CodeQuiz - UML Use Case Diagrams

## 1. System Overview Use Case Diagram

```mermaid
flowchart LR
    subgraph actors_left[" "]
        Examiner((Examiner))
    end

    subgraph system["CodeQuiz System"]
        UC1([Register])
        UC2([Login])
        UC3([Logout])
        UC4([Reset Password])
        UC5([Create Quiz])
        UC6([Add Questions])
        UC7([Configure Quiz Settings])
        UC8([View Created Quizzes])
        UC9([Update Quiz])
        UC10([Delete Quiz])
        UC11([View Quiz Attempts])
        UC12([Join Quiz])
        UC13([Take Quiz Attempt])
        UC14([Submit Attempt])
        UC15([Run Code])
        UC16([View Attempt History])
        UC17([View Grades])
    end

    subgraph actors_right[" "]
        Examinee((Examinee))
        System((System))
    end

    %% Examiner connections
    Examiner --> UC1
    Examiner --> UC2
    Examiner --> UC3
    Examiner --> UC4
    Examiner --> UC5
    Examiner --> UC8
    Examiner --> UC9
    Examiner --> UC10
    Examiner --> UC11

    %% Examinee connections
    Examinee --> UC1
    Examinee --> UC2
    Examinee --> UC3
    Examinee --> UC4
    Examinee --> UC12
    Examinee --> UC13
    Examinee --> UC14
    Examinee --> UC16
    Examinee --> UC17

    %% System connections
    System --> UC15

    %% Include relationships
    UC5 -.->|<<include>>| UC6
    UC5 -.->|<<include>>| UC7
    UC13 -.->|<<include>>| UC15
```

---

## 2. Authentication Module Use Case Diagram

```mermaid
flowchart LR
    subgraph actors[" "]
        User((User))
    end

    subgraph auth_system["Authentication System"]
        UC1([Register])
        UC2([Login])
        UC3([Logout])
        UC4([Refresh Token])
        UC5([Forget Password Request])
        UC6([Reset Password])
        UC7([Update Profile])
        UC8([Search Users])
        UC9([Check Username Availability])
    end

    User --> UC1
    User --> UC2
    User --> UC3
    User --> UC4
    User --> UC5
    User --> UC6
    User --> UC7
    User --> UC8

    %% Include relationships
    UC1 -.->|<<include>>| UC9
    UC5 -.->|<<include>>| UC6

    %% Extend relationships
    UC2 -.->|<<extend>>| UC4
```

---

## 3. Examiner Use Case Diagram (Quiz Management)

```mermaid
flowchart LR
    subgraph actors[" "]
        Examiner((Examiner))
    end

    subgraph quiz_management["Quiz Management System"]
        UC1([Create Quiz])
        UC2([Add Questions])
        UC3([Define Test Cases])
        UC4([Configure Quiz Settings])
        UC5([Set Duration])
        UC6([Set Availability Period])
        UC7([Set Programming Language])
        UC8([Allow Multiple Attempts])
        UC9([Configure Output Settings])
        UC10([View Created Quizzes])
        UC11([Update Quiz])
        UC12([Delete Quiz])
        UC13([View Quiz Attempts])
        UC14([View Examinee Solutions])
        UC15([View Evaluation Results])
        UC16([Share Quiz Code])
    end

    Examiner --> UC1
    Examiner --> UC10
    Examiner --> UC11
    Examiner --> UC12
    Examiner --> UC13
    Examiner --> UC16

    %% Include relationships
    UC1 -.->|<<include>>| UC2
    UC1 -.->|<<include>>| UC4
    UC2 -.->|<<include>>| UC3
    UC4 -.->|<<include>>| UC5
    UC4 -.->|<<include>>| UC6
    UC4 -.->|<<include>>| UC7

    %% Extend relationships
    UC4 -.->|<<extend>>| UC8
    UC4 -.->|<<extend>>| UC9
    UC13 -.->|<<extend>>| UC14
    UC14 -.->|<<extend>>| UC15
```

---

## 4. Examinee Use Case Diagram (Quiz Participation)

```mermaid
flowchart LR
    subgraph actors_left[" "]
        Examinee((Examinee))
    end

    subgraph quiz_participation["Quiz Participation System"]
        UC1([Join Quiz by Code])
        UC2([Begin Attempt])
        UC3([View Questions])
        UC4([Write Code Solution])
        UC5([Run Code])
        UC6([View Output])
        UC7([Navigate Questions])
        UC8([Save Solution])
        UC9([Submit Attempt])
        UC10([View Attempt History])
        UC11([Continue Unfinished Attempt])
        UC12([View Grades])
        UC13([View Evaluation Results])
    end

    subgraph actors_right[" "]
        System((System))
    end

    Examinee --> UC1
    Examinee --> UC10
    Examinee --> UC11
    Examinee --> UC12

    %% System actor
    System --> UC9

    %% Include relationships
    UC1 -.->|<<include>>| UC2
    UC2 -.->|<<include>>| UC3
    UC3 -.->|<<include>>| UC4
    UC4 -.->|<<include>>| UC8
    UC9 -.->|<<include>>| UC8

    %% Extend relationships
    UC4 -.->|<<extend>>| UC5
    UC5 -.->|<<extend>>| UC6
    UC3 -.->|<<extend>>| UC7
    UC12 -.->|<<extend>>| UC13
```

---

## 5. Code Execution & Evaluation Use Case Diagram

```mermaid
flowchart LR
    subgraph actors_left[" "]
        Examinee((Examinee))
    end

    subgraph execution_system["Code Execution & Evaluation System"]
        UC1([Run Code])
        UC2([Execute in Sandbox])
        UC3([Return Output])
        UC4([Return Errors])
        UC5([Submit Attempt])
        UC6([Evaluate Solutions])
        UC7([Run Test Cases])
        UC8([Calculate Grade])
        UC9([Auto-Submit on Timeout])
    end

    subgraph actors_right[" "]
        System((System))
    end

    Examinee --> UC1
    Examinee --> UC5

    System --> UC9
    System --> UC6

    %% Include relationships
    UC1 -.->|<<include>>| UC2
    UC2 -.->|<<include>>| UC3
    UC5 -.->|<<include>>| UC6
    UC6 -.->|<<include>>| UC7
    UC7 -.->|<<include>>| UC8
    UC9 -.->|<<include>>| UC5

    %% Extend relationships
    UC2 -.->|<<extend>>| UC4
```

---

## 6. Complete System Use Case Diagram (PlantUML Format)

For more traditional UML rendering, here's the PlantUML version:

```plantuml
@startuml CodeQuiz_UseCaseDiagram

left to right direction
skinparam packageStyle rectangle
skinparam actorStyle awesome

actor "Examiner" as examiner
actor "Examinee" as examinee
actor "System" as system

rectangle "CodeQuiz System" {
    
    package "Authentication" {
        usecase "Register" as UC_REG
        usecase "Login" as UC_LOGIN
        usecase "Logout" as UC_LOGOUT
        usecase "Reset Password" as UC_RESET
        usecase "Refresh Token" as UC_REFRESH
    }
    
    package "Quiz Participation" {
        usecase "Join Quiz" as UC_JOIN
        usecase "Begin Attempt" as UC_BEGIN
        usecase "Answer Questions" as UC_ANSWER
        usecase "Run Code" as UC_RUN
        usecase "Submit Attempt" as UC_SUBMIT
        usecase "View History" as UC_HIST
        usecase "View Grades" as UC_GRADES
    }
    
    package "Evaluation" {
        usecase "Execute Code" as UC_EXEC
        usecase "Evaluate Solutions" as UC_EVAL
        usecase "Auto-Submit" as UC_AUTO
    }
    
    package "Quiz Management" {
        usecase "Create Quiz" as UC_CREATE
        usecase "Add Questions" as UC_ADDQ
        usecase "Define Test Cases" as UC_TESTS
        usecase "Configure Settings" as UC_CONFIG
        usecase "View Quizzes" as UC_VIEW
        usecase "Update Quiz" as UC_UPDATE
        usecase "Delete Quiz" as UC_DELETE
        usecase "View Attempts" as UC_VATTEMPTS
        usecase "View Solutions" as UC_VSOL
    }
}

' Examinee associations
examinee --> UC_REG
examinee --> UC_LOGIN
examinee --> UC_LOGOUT
examinee --> UC_RESET
examinee --> UC_JOIN
examinee --> UC_SUBMIT
examinee --> UC_HIST
examinee --> UC_GRADES

' System associations
system --> UC_EXEC
system --> UC_EVAL
system --> UC_AUTO

' Examiner associations
examiner --> UC_REG
examiner --> UC_LOGIN
examiner --> UC_LOGOUT
examiner --> UC_RESET
examiner --> UC_CREATE
examiner --> UC_VIEW
examiner --> UC_UPDATE
examiner --> UC_DELETE
examiner --> UC_VATTEMPTS
examiner --> UC_EVAL

' Include relationships
UC_CREATE ..> UC_ADDQ : <<include>>
UC_ADDQ ..> UC_TESTS : <<include>>
UC_CREATE ..> UC_CONFIG : <<include>>
UC_JOIN ..> UC_BEGIN : <<include>>
UC_BEGIN ..> UC_ANSWER : <<include>>
UC_SUBMIT ..> UC_EVAL : <<include>>
UC_EVAL ..> UC_EXEC : <<include>>
UC_AUTO ..> UC_SUBMIT : <<include>>
UC_VATTEMPTS ..> UC_VSOL : <<include>>

' Extend relationships
UC_ANSWER ..> UC_RUN : <<extend>>
UC_LOGIN ..> UC_REFRESH : <<extend>>

@enduml
```

---

## 7. Draw.io XML Format

You can import this XML directly into draw.io:

```xml
<mxfile host="app.diagrams.net">
  <diagram name="CodeQuiz Use Cases" id="use-case-main">
    <mxGraphModel dx="1434" dy="780" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1169" pageHeight="827" math="0" shadow="0">
      <root>
        <mxCell id="0" />
        <mxCell id="1" parent="0" />
        
        <!-- System Boundary -->
        <mxCell id="system" value="CodeQuiz System" style="shape=umlFrame;whiteSpace=wrap;html=1;width=120;height=30;boundedLbl=1;verticalAlign=middle;align=left;spacingLeft=5;fillColor=#dae8fc;strokeColor=#6c8ebf;" vertex="1" parent="1">
          <mxGeometry x="200" y="40" width="700" height="700" as="geometry" />
        </mxCell>
        
        <!-- Actors -->
        <mxCell id="examiner" value="Examiner" style="shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;html=1;outlineConnect=0;" vertex="1" parent="1">
          <mxGeometry x="60" y="200" width="30" height="60" as="geometry" />
        </mxCell>
        
        <mxCell id="examinee" value="Examinee" style="shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;html=1;outlineConnect=0;" vertex="1" parent="1">
          <mxGeometry x="60" y="450" width="30" height="60" as="geometry" />
        </mxCell>
        
        <mxCell id="system_actor" value="System" style="shape=umlActor;verticalLabelPosition=bottom;verticalAlign=top;html=1;outlineConnect=0;" vertex="1" parent="1">
          <mxGeometry x="980" y="350" width="30" height="60" as="geometry" />
        </mxCell>
        
        <!-- Authentication Use Cases -->
        <mxCell id="uc_register" value="Register" style="ellipse;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="1">
          <mxGeometry x="250" y="70" width="100" height="50" as="geometry" />
        </mxCell>
        
        <mxCell id="uc_login" value="Login" style="ellipse;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="1">
          <mxGeometry x="380" y="70" width="100" height="50" as="geometry" />
        </mxCell>
        
        <mxCell id="uc_logout" value="Logout" style="ellipse;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="1">
          <mxGeometry x="510" y="70" width="100" height="50" as="geometry" />
        </mxCell>
        
        <mxCell id="uc_reset" value="Reset Password" style="ellipse;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;" vertex="1" parent="1">
          <mxGeometry x="640" y="70" width="100" height="50" as="geometry" />
        </mxCell>
        
        <!-- Quiz Participation Use Cases -->
        <mxCell id="uc_join" value="Join Quiz" style="ellipse;whiteSpace=wrap;html=1;fillColor=#e1d5e7;strokeColor=#9673a6;" vertex="1" parent="1">
          <mxGeometry x="250" y="400" width="100" height="50" as="geometry" />
        </mxCell>
        
        <mxCell id="uc_begin" value="Begin Attempt" style="ellipse;whiteSpace=wrap;html=1;fillColor=#e1d5e7;strokeColor=#9673a6;" vertex="1" parent="1">
          <mxGeometry x="400" y="400" width="100" height="50" as="geometry" />
        </mxCell>
        
        <mxCell id="uc_answer" value="Answer Questions" style="ellipse;whiteSpace=wrap;html=1;fillColor=#e1d5e7;strokeColor=#9673a6;" vertex="1" parent="1">
          <mxGeometry x="550" y="400" width="110" height="50" as="geometry" />
        </mxCell>
        
        <mxCell id="uc_run" value="Run Code" style="ellipse;whiteSpace=wrap;html=1;fillColor=#e1d5e7;strokeColor=#9673a6;" vertex="1" parent="1">
          <mxGeometry x="700" y="400" width="100" height="50" as="geometry" />
        </mxCell>
        
        <mxCell id="uc_submit" value="Submit Attempt" style="ellipse;whiteSpace=wrap;html=1;fillColor=#e1d5e7;strokeColor=#9673a6;" vertex="1" parent="1">
          <mxGeometry x="250" y="480" width="100" height="50" as="geometry" />
        </mxCell>
        
        <mxCell id="uc_history" value="View History" style="ellipse;whiteSpace=wrap;html=1;fillColor=#e1d5e7;strokeColor=#9673a6;" vertex="1" parent="1">
          <mxGeometry x="380" y="480" width="100" height="50" as="geometry" />
        </mxCell>
        
        <mxCell id="uc_grades" value="View Grades" style="ellipse;whiteSpace=wrap;html=1;fillColor=#e1d5e7;strokeColor=#9673a6;" vertex="1" parent="1">
          <mxGeometry x="510" y="480" width="100" height="50" as="geometry" />
        </mxCell>
        
        <!-- Evaluation Use Cases -->
        <mxCell id="uc_exec" value="Execute Code" style="ellipse;whiteSpace=wrap;html=1;fillColor=#f8cecc;strokeColor=#b85450;" vertex="1" parent="1">
          <mxGeometry x="550" y="580" width="100" height="50" as="geometry" />
        </mxCell>
        
        <mxCell id="uc_eval" value="Evaluate Solutions" style="ellipse;whiteSpace=wrap;html=1;fillColor=#f8cecc;strokeColor=#b85450;" vertex="1" parent="1">
          <mxGeometry x="700" y="580" width="110" height="50" as="geometry" />
        </mxCell>
        
        <mxCell id="uc_auto" value="Auto-Submit on Timeout" style="ellipse;whiteSpace=wrap;html=1;fillColor=#f8cecc;strokeColor=#b85450;" vertex="1" parent="1">
          <mxGeometry x="700" y="660" width="130" height="50" as="geometry" />
        </mxCell>
        
      </root>
    </mxGraphModel>
  </diagram>
</mxfile>
```

---

## Use Case Descriptions Summary

| ID | Use Case | Actor(s) | Description |
|----|----------|----------|-------------|
| UC1 | Register | User | Create a new account in the system |
| UC2 | Login | User | Authenticate and access the system |
| UC3 | Logout | User | End the current session |
| UC4 | Reset Password | User | Recover account access |
| UC5 | Create Quiz | Examiner | Create a new quiz with questions |
| UC6 | Add Questions | Examiner | Add coding questions to a quiz |
| UC7 | Configure Settings | Examiner | Set quiz duration, language, etc. |
| UC8 | View Quizzes | Examiner | List all created quizzes |
| UC9 | Update Quiz | Examiner | Modify existing quiz |
| UC10 | Delete Quiz | Examiner | Remove a quiz from the system |
| UC11 | View Attempts | Examiner | See all attempts for a quiz |
| UC12 | Join Quiz | Examinee | Enter a quiz using a code |
| UC13 | Begin Attempt | Examinee | Start taking a quiz |
| UC14 | Answer Questions | Examinee | Write code solutions |
| UC15 | Run Code | Examinee | Execute code to test solutions |
| UC16 | Submit Attempt | Examinee/System | Finalize and submit the attempt |
| UC17 | View History | Examinee | See past quiz attempts |
| UC18 | View Grades | Examinee | Check scores and results |
| UC19 | Execute Code | System | Run code in sandboxed environment |
| UC20 | Evaluate Solutions | System | Run test cases and grade |
| UC21 | Auto-Submit | System | Submit when time expires |

