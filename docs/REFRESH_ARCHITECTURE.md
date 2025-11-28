# Course Material Refresh - Architecture Overview

## System Architecture Diagram

```mermaid
graph TB
    subgraph "Current State"
        A[Aspire 9.3.1]
        B[Semantic Kernel 1.61.0]
        C[5 Notebooks]
        D[4 Orchestration Patterns]
    end
    
    subgraph "Target State"
        E[Aspire 13.x]
        F[Latest SK Version]
        G[7+ Notebooks]
        H[8+ Orchestration Patterns]
    end
    
    A -->|Update| E
    B -->|Update| F
    C -->|Expand| G
    D -->|Add Patterns| H
    
    style E fill:#90EE90
    style F fill:#90EE90
    style G fill:#90EE90
    style H fill:#90EE90
```

## Content Expansion Map

```mermaid
mindmap
  root((Workshop Material))
    Existing Content
      Notebooks 0-5
        Setup
        SK Intro
        Agents
        Plugins & MCP
        OpenAPI
        Orchestration
        Chat History
      C# Solution
        AppHost
        Server
        VS Code Extension
      Documentation
        README
        Assignments 1-3
    New Content
      Notebook 6: A2A Protocol
        Direct Communication
        Message Passing
        Event-Driven
        Best Practices
      Notebook 7: Process Framework
        Stateful Workflows
        HITL Patterns
        Approval Gates
        Error Handling
      Enhanced Orchestration
        Aggregator Pattern
        Reducer Pattern
        Scatter-Gather Pattern
        Pipeline Pattern
      New Assignments
        Assignment 4: A2A
        Assignment 5: HITL Process
```

## Update Workflow

```mermaid
flowchart LR
    Start([Start]) --> Phase1[Phase 1:<br/>Version Updates]
    Phase1 --> Test1{Tests Pass?}
    Test1 -->|No| Fix1[Fix Issues]
    Fix1 --> Test1
    Test1 -->|Yes| Phase2[Phase 2:<br/>A2A Protocol]
    
    Phase2 --> Test2{Tests Pass?}
    Test2 -->|No| Fix2[Fix Issues]
    Fix2 --> Test2
    Test2 -->|Yes| Phase3[Phase 3:<br/>Orchestration]
    
    Phase3 --> Test3{Tests Pass?}
    Test3 -->|No| Fix3[Fix Issues]
    Fix3 --> Test3
    Test3 -->|Yes| Phase4[Phase 4:<br/>Process Framework]
    
    Phase4 --> Test4{Tests Pass?}
    Test4 -->|No| Fix4[Fix Issues]
    Fix4 --> Test4
    Test4 -->|Yes| Phase5[Phase 5:<br/>Documentation]
    
    Phase5 --> Final{Final<br/>Validation?}
    Final -->|No| Refine[Refine Content]
    Refine --> Final
    Final -->|Yes| Complete([Complete])
    
    style Start fill:#87CEEB
    style Complete fill:#90EE90
    style Phase1 fill:#FFE4B5
    style Phase2 fill:#FFE4B5
    style Phase3 fill:#FFE4B5
    style Phase4 fill:#FFE4B5
    style Phase5 fill:#FFE4B5
```

## Package Dependency Flow

```mermaid
graph TD
    subgraph "Solution Packages"
        DP[Directory.Packages.props]
        DP --> SK[Semantic Kernel 1.61.0 → Latest]
        DP --> ASP[Aspire 9.3.1 → 13.x]
        DP --> AGENTS[SK Agents Packages]
        DP --> CONNECTORS[SK Connectors]
    end
    
    subgraph "Notebook Packages"
        NB1[Notebook 1] --> SK
        NB2[Notebook 2] --> SK
        NB2 --> AGENTS
        NB3[Notebook 3] --> SK
        NB3 --> AGENTS
        NB4[Notebook 4] --> SK
        NB4 --> AGENTS
        NB5[Notebook 5] --> SK
        NB5 --> AGENTS
        NB6[Notebook 6 NEW] --> SK
        NB6 --> AGENTS
        NB7[Notebook 7 NEW] --> SK
        NB7 --> AGENTS
        NB7 --> PROCESS[SK Process Framework]
    end
    
    SK --> TESTS[Unit Tests]
    ASP --> TESTS
    AGENTS --> TESTS
    
    style SK fill:#FFB6C1
    style ASP fill:#FFB6C1
    style NB6 fill:#90EE90
    style NB7 fill:#90EE90
    style PROCESS fill:#90EE90
```

## Orchestration Patterns Evolution

```mermaid
graph LR
    subgraph "Current Patterns"
        C1[Concurrent]
        C2[Sequential]
        C3[Group Chat]
        C4[Custom]
    end
    
    subgraph "New Patterns"
        N1[Aggregator]
        N2[Reducer]
        N3[Scatter-Gather]
        N4[Pipeline]
    end
    
    C1 -.->|Expand| ALL[Complete Pattern Library]
    C2 -.->|Expand| ALL
    C3 -.->|Expand| ALL
    C4 -.->|Expand| ALL
    N1 -->|Add| ALL
    N2 -->|Add| ALL
    N3 -->|Add| ALL
    N4 -->|Add| ALL
    
    style N1 fill:#90EE90
    style N2 fill:#90EE90
    style N3 fill:#90EE90
    style N4 fill:#90EE90
    style ALL fill:#FFD700
```

## Learning Path Integration

```mermaid
graph TD
    Start([Student Starts]) --> NB0[0: AI Settings]
    NB0 --> NB1[1: SK Introduction]
    NB1 --> NB2[2: Agents Basics]
    NB2 --> NB3[3: Plugins & MCP]
    NB3 --> NB3_1[3.1: OpenAPI Plugin]
    NB3_1 --> NB4[4: Orchestration]
    NB4 --> NB5[5: Chat History]
    NB5 --> NB6[6: A2A Protocol NEW]
    NB6 --> NB7[7: Process Framework NEW]
    NB7 --> ASG1[Assignment 1:<br/>Three Agents]
    ASG1 --> ASG2[Assignment 2:<br/>Plugins & MCP]
    ASG2 --> ASG3[Assignment 3:<br/>Orchestration]
    ASG3 --> ASG4[Assignment 4:<br/>A2A Protocol NEW]
    ASG4 --> ASG5[Assignment 5:<br/>HITL Process NEW]
    ASG5 --> PROJECT[Final Project:<br/>Complete System]
    PROJECT --> GRADUATE([Graduate])
    
    style NB6 fill:#90EE90
    style NB7 fill:#90EE90
    style ASG4 fill:#90EE90
    style ASG5 fill:#90EE90
    style Start fill:#87CEEB
    style GRADUATE fill:#FFD700
```

## New Content Structure

```mermaid
graph TD
    subgraph "Notebook 6: A2A Protocol"
        A1[Introduction to A2A]
        A2[Core Concepts]
        A3[Implementation Patterns]
        A4[Practical Examples]
        A5[Best Practices]
        A1 --> A2 --> A3 --> A4 --> A5
    end
    
    subgraph "Notebook 7: Process Framework"
        P1[Process Framework Intro]
        P2[Stateful Workflows]
        P3[HITL Introduction]
        P4[HITL Patterns]
        P5[Real-World Example]
        P1 --> P2 --> P3 --> P4 --> P5
    end
    
    subgraph "Enhanced Notebook 4"
        O1[Existing 4 Patterns]
        O2[Aggregator Pattern]
        O3[Reducer Pattern]
        O4[Scatter-Gather Pattern]
        O5[Pattern Comparison]
        O1 --> O2 --> O3 --> O4 --> O5
    end
    
    style A1 fill:#FFE4B5
    style P1 fill:#FFE4B5
    style O2 fill:#90EE90
    style O3 fill:#90EE90
    style O4 fill:#90EE90
```

## Testing Strategy

```mermaid
graph TD
    START([Begin Testing]) --> UNIT[Unit Tests]
    UNIT --> INT[Integration Tests]
    INT --> NB[Notebook Execution Tests]
    NB --> BUILD[Build Tests]
    BUILD --> E2E[End-to-End Tests]
    
    UNIT -.-> U1[Package Compatibility]
    UNIT -.-> U2[API Changes]
    
    INT -.-> I1[Aspire Integration]
    INT -.-> I2[SK Agent Integration]
    
    NB -.-> N1[All Notebooks Run]
    NB -.-> N2[Code Examples Work]
    
    BUILD -.-> B1[Solution Builds]
    BUILD -.-> B2[No Warnings]
    
    E2E -.-> E1[Workshop Flow]
    E2E -.-> E2[Assignment Completion]
    
    E2E --> PASS{All Pass?}
    PASS -->|Yes| SUCCESS([Release Ready])
    PASS -->|No| FIX[Fix Issues]
    FIX --> START
    
    style SUCCESS fill:#90EE90
    style START fill:#87CEEB
```

## Risk Management Matrix

| Risk Level | Area | Mitigation |
|------------|------|------------|
| 🔴 High | Aspire 13 Breaking Changes | Maintain 9.3.1 fallback branch |
| 🟡 Medium | SK API Changes | Early testing, comprehensive docs |
| 🟡 Medium | Notebook Compatibility | Version-pinned examples |
| 🟢 Low | Documentation Updates | Clear change logs |
| 🟢 Low | Assignment Difficulty | Gradual complexity increase |

## Success Metrics Dashboard

```mermaid
graph LR
    subgraph "Completion Metrics"
        M1[Version Updates: 0%]
        M2[New Notebooks: 0%]
        M3[Orchestration Patterns: 50%]
        M4[Documentation: 0%]
        M5[Testing: 0%]
    end
    
    M1 --> TARGET1[Target: 100%]
    M2 --> TARGET2[Target: 100%]
    M3 --> TARGET3[Target: 100%]
    M4 --> TARGET4[Target: 100%]
    M5 --> TARGET5[Target: 100%]
    
    style TARGET1 fill:#90EE90
    style TARGET2 fill:#90EE90
    style TARGET3 fill:#90EE90
    style TARGET4 fill:#90EE90
    style TARGET5 fill:#90EE90
```

## Implementation Phases Timeline

```mermaid
gantt
    title Course Material Refresh Timeline
    dateFormat YYYY-MM-DD
    section Phase 1: Versions
    Research & Planning           :p1a, 2025-01-01, 2d
    Update Packages               :p1b, after p1a, 2d
    Testing & Fixes               :p1c, after p1b, 1d
    
    section Phase 2: A2A
    Create Notebook 6             :p2a, after p1c, 2d
    Add C# Examples               :p2b, after p2a, 2d
    Testing                       :p2c, after p2b, 1d
    
    section Phase 3: Orchestration
    Add New Patterns              :p3a, after p2c, 3d
    Testing                       :p3b, after p3a, 2d
    
    section Phase 4: Process Framework
    Create Notebook 7             :p4a, after p3b, 3d
    Add C# Examples               :p4b, after p4a, 2d
    
    section Phase 5: Documentation
    Update Docs                   :p5a, after p4b, 2d
    Create Assignments            :p5b, after p5a, 2d
    Final Testing                 :p5c, after p5b, 1d
```

---

*This architecture document provides visual representations of the refresh strategy and serves as a reference throughout the implementation process.*