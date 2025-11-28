# Assignment 5: Process Framework and Human-in-the-Loop Implementation

## Objective
Learn to build event-driven business processes using the Semantic Kernel Process Framework, and implement Human-in-the-Loop (HITL) patterns for quality assurance and compliance. You'll create a production-ready workflow that combines AI automation with human oversight.

## Prerequisites
- Complete Assignments 1-4
- Review [Notebook 7: Process Framework & HITL](../../notebooks/7-Process-Framework-and-HITL.ipynb)
- Understanding of event-driven architecture
- Familiarity with workflow systems

## Background: Process Framework & HITL

### Process Framework
The Semantic Kernel Process Framework enables you to build complex, event-driven business processes where each step can:
- Invoke AI agents
- Execute native code
- Emit events to trigger subsequent steps
- Branch based on conditions
- Wait for external input (including human approval)

### Human-in-the-Loop (HITL)
HITL is a design pattern where human oversight is integrated into AI workflows for:
- **Quality Assurance**: Review AI outputs before they're used
- **Compliance**: Ensure regulatory requirements are met
- **Safety**: Prevent harmful or incorrect AI actions
- **Learning**: Gather feedback to improve AI models

## Requirements

### Part 1: Build a Code Review Process

Create an automated code review workflow with the following steps:

```
┌─────────────────────────────────────────────────────────────┐
│                    Code Review Process                       │
│  ┌──────────┐      ┌──────────┐      ┌──────────┐         │
│  │ Analyze  │─────▶│ Generate │─────▶│  Human   │         │
│  │  Code    │      │  Report  │      │  Review  │         │
│  └────┬─────┘      └────┬─────┘      └────┬─────┘         │
│       │ Event           │ Event           │ Event         │
│       │ (Analyzed)      │ (Generated)     │ (Approved/    │
│       │                 │                 │  Rejected)    │
│       ▼                 ▼                 ▼               │
│  ┌──────────┐      ┌──────────┐      ┌──────────┐         │
│  │ Security │      │  Apply   │      │  Notify  │         │
│  │  Check   │      │  Changes │      │   Team   │         │
│  └──────────┘      └──────────┘      └──────────┘         │
└─────────────────────────────────────────────────────────────┘
```

#### Step 1: Code Analysis Step
```csharp
public class CodeAnalysisStep : KernelProcessStep
{
    public static class Functions
    {
        public const string Analyze = nameof(Analyze);
    }

    [KernelFunction(Functions.Analyze)]
    public async Task AnalyzeAsync(
        KernelProcessStepContext context,
        Kernel kernel,
        string codeToAnalyze)
    {
        // Analyze code for:
        // - Code quality issues
        // - Security vulnerabilities
        // - Performance concerns
        // - Best practice violations
        
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        var prompt = $@"
            Analyze the following code for:
            1. Code quality issues
            2. Security vulnerabilities
            3. Performance concerns
            4. Best practice violations
            
            Code:
            {codeToAnalyze}
            
            Provide a structured analysis with severity ratings.";
        
        var result = await chatService.GetChatMessageContentAsync(prompt);
        var analysis = result.Content;
        
        // Emit event with analysis results
        await context.EmitEventAsync(new KernelProcessEvent
        {
            Id = ProcessEvents.CodeAnalyzed,
            Data = new CodeAnalysisResult
            {
                Code = codeToAnalyze,
                Analysis = analysis,
                Timestamp = DateTime.UtcNow
            }
        });
    }
}
```

#### Step 2: Report Generation Step
```csharp
public class ReportGenerationStep : KernelProcessStep
{
    public static class Functions
    {
        public const string Generate = nameof(Generate);
    }

    [KernelFunction(Functions.Generate)]
    public async Task GenerateAsync(
        KernelProcessStepContext context,
        Kernel kernel,
        CodeAnalysisResult analysisResult)
    {
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        var prompt = $@"
            Based on this code analysis, create a comprehensive review report:
            
            Analysis: {analysisResult.Analysis}
            
            Include:
            1. Executive Summary
            2. Critical Issues (if any)
            3. Recommendations
            4. Suggested Actions
            
            Format as a professional code review report.";
        
        var result = await chatService.GetChatMessageContentAsync(prompt);
        
        await context.EmitEventAsync(new KernelProcessEvent
        {
            Id = ProcessEvents.ReportGenerated,
            Data = new CodeReviewReport
            {
                Code = analysisResult.Code,
                Analysis = analysisResult.Analysis,
                Report = result.Content,
                GeneratedAt = DateTime.UtcNow
            }
        });
    }
}
```

#### Step 3: Human Review Step (HITL)
```csharp
public class HumanReviewStep : KernelProcessStep
{
    private readonly IReviewQueueService _reviewQueue;
    private readonly INotificationService _notificationService;
    
    public static class Functions
    {
        public const string RequestReview = nameof(RequestReview);
    }

    [KernelFunction(Functions.RequestReview)]
    public async Task RequestReviewAsync(
        KernelProcessStepContext context,
        CodeReviewReport report)
    {
        // Create review request
        var reviewRequest = new ReviewRequest
        {
            Id = Guid.NewGuid(),
            Report = report,
            Status = ReviewStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        
        // Store in review queue
        await _reviewQueue.EnqueueAsync(reviewRequest);
        
        // Notify reviewers
        await _notificationService.NotifyReviewersAsync(new ReviewNotification
        {
            ReviewId = reviewRequest.Id,
            Subject = "Code Review Required",
            Priority = DeterminePriority(report),
            Message = $"A code review is pending. Report generated at {report.GeneratedAt}"
        });
        
        // Process pauses here until human responds
        // Will be resumed via webhook or API call
        
        Console.WriteLine($"[HITL] Review request {reviewRequest.Id} created and sent to reviewers");
        Console.WriteLine($"[HITL] Waiting for human approval...");
        
        // In production, this would wait for external event
        // For demo, we'll simulate immediate response
        var decision = await SimulateHumanReviewAsync(reviewRequest);
        
        if (decision.Approved)
        {
            await context.EmitEventAsync(new KernelProcessEvent
            {
                Id = ProcessEvents.ReviewApproved,
                Data = new ReviewDecision
                {
                    ReviewId = reviewRequest.Id,
                    Approved = true,
                    Comments = decision.Comments,
                    ReviewedBy = decision.ReviewedBy,
                    ReviewedAt = DateTime.UtcNow
                }
            });
        }
        else
        {
            await context.EmitEventAsync(new KernelProcessEvent
            {
                Id = ProcessEvents.ReviewRejected,
                Data = new ReviewDecision
                {
                    ReviewId = reviewRequest.Id,
                    Approved = false,
                    Comments = decision.Comments,
                    ReviewedBy = decision.ReviewedBy,
                    ReviewedAt = DateTime.UtcNow,
                    RequiredActions = decision.RequiredActions
                }
            });
        }
    }
    
    private async Task<ReviewDecision> SimulateHumanReviewAsync(ReviewRequest request)
    {
        // Simulate review time
        await Task.Delay(1000);
        
        // Simulate approval decision (in production, this comes from real human via API)
        return new ReviewDecision
        {
            Approved = true, // Change to test rejection flow
            Comments = "Analysis looks good. Approved for implementation.",
            ReviewedBy = "senior.developer@company.com",
            ReviewedAt = DateTime.UtcNow
        };
    }
    
    private ReviewPriority DeterminePriority(CodeReviewReport report)
    {
        // Analyze report to determine urgency
        if (report.Analysis.Contains("critical", StringComparison.OrdinalIgnoreCase) ||
            report.Analysis.Contains("security", StringComparison.OrdinalIgnoreCase))
        {
            return ReviewPriority.High;
        }
        return ReviewPriority.Normal;
    }
}
```

#### Step 4: Apply Changes Step
```csharp
public class ApplyChangesStep : KernelProcessStep
{
    public static class Functions
    {
        public const string Apply = nameof(Apply);
    }

    [KernelFunction(Functions.Apply)]
    public async Task ApplyAsync(
        KernelProcessStepContext context,
        Kernel kernel,
        ReviewDecision decision,
        CodeReviewReport report)
    {
        if (!decision.Approved)
        {
            Console.WriteLine("[ApplyChanges] Review was rejected. No changes applied.");
            return;
        }
        
        Console.WriteLine("[ApplyChanges] Review approved. Applying recommendations...");
        
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        var prompt = $@"
            Based on this approved code review, generate improved code:
            
            Original Code: {report.Code}
            Review Analysis: {report.Analysis}
            Review Comments: {decision.Comments}
            
            Provide the improved code with comments explaining changes.";
        
        var result = await chatService.GetChatMessageContentAsync(prompt);
        
        await context.EmitEventAsync(new KernelProcessEvent
        {
            Id = ProcessEvents.ChangesApplied,
            Data = new AppliedChanges
            {
                ImprovedCode = result.Content,
                AppliedAt = DateTime.UtcNow,
                ApprovedBy = decision.ReviewedBy
            }
        });
    }
}
```

### Part 2: Build the Process

Wire up all the steps into a cohesive process:

```csharp
public class CodeReviewProcess
{
    public static async Task<KernelProcess> BuildProcessAsync(Kernel kernel)
    {
        var processBuilder = new ProcessBuilder("CodeReviewProcess");
        
        // Add steps
        var analysisStep = processBuilder.AddStepFromType<CodeAnalysisStep>();
        var reportStep = processBuilder.AddStepFromType<ReportGenerationStep>();
        var reviewStep = processBuilder.AddStepFromType<HumanReviewStep>();
        var applyStep = processBuilder.AddStepFromType<ApplyChangesStep>();
        var notifyStep = processBuilder.AddStepFromType<NotificationStep>();
        
        // Wire up event flow
        processBuilder
            .OnInputEvent(ProcessEvents.StartReview)
            .SendEventTo(new ProcessFunctionTargetBuilder(
                analysisStep,
                CodeAnalysisStep.Functions.Analyze,
                parameterName: "codeToAnalyze"));
        
        analysisStep
            .OnEvent(ProcessEvents.CodeAnalyzed)
            .SendEventTo(new ProcessFunctionTargetBuilder(
                reportStep,
                ReportGenerationStep.Functions.Generate,
                parameterName: "analysisResult"));
        
        reportStep
            .OnEvent(ProcessEvents.ReportGenerated)
            .SendEventTo(new ProcessFunctionTargetBuilder(
                reviewStep,
                HumanReviewStep.Functions.RequestReview,
                parameterName: "report"));
        
        // Handle approval
        reviewStep
            .OnEvent(ProcessEvents.ReviewApproved)
            .SendEventTo(new ProcessFunctionTargetBuilder(
                applyStep,
                ApplyChangesStep.Functions.Apply,
                parameterName: "decision"));
        
        // Handle rejection
        reviewStep
            .OnEvent(ProcessEvents.ReviewRejected)
            .SendEventTo(new ProcessFunctionTargetBuilder(
                notifyStep,
                NotificationStep.Functions.NotifyRejection,
                parameterName: "decision"));
        
        applyStep
            .OnEvent(ProcessEvents.ChangesApplied)
            .StopProcess();
        
        notifyStep
            .OnEvent(ProcessEvents.TeamNotified)
            .StopProcess();
        
        return processBuilder.Build();
    }
}
```

### Part 3: Implement Review Queue Service

Create a service to manage review requests:

```csharp
public interface IReviewQueueService
{
    Task EnqueueAsync(ReviewRequest request);
    Task<ReviewRequest?> GetPendingReviewAsync(Guid reviewId);
    Task<IEnumerable<ReviewRequest>> GetPendingReviewsAsync();
    Task UpdateReviewStatusAsync(Guid reviewId, ReviewStatus status);
}

public class ReviewQueueService : IReviewQueueService
{
    private readonly ConcurrentDictionary<Guid, ReviewRequest> _reviews = new();
    
    public Task EnqueueAsync(ReviewRequest request)
    {
        _reviews.TryAdd(request.Id, request);
        return Task.CompletedTask;
    }
    
    public Task<ReviewRequest?> GetPendingReviewAsync(Guid reviewId)
    {
        _reviews.TryGetValue(reviewId, out var request);
        return Task.FromResult(request);
    }
    
    public Task<IEnumerable<ReviewRequest>> GetPendingReviewsAsync()
    {
        var pending = _reviews.Values
            .Where(r => r.Status == ReviewStatus.Pending)
            .OrderByDescending(r => r.CreatedAt);
        return Task.FromResult(pending);
    }
    
    public Task UpdateReviewStatusAsync(Guid reviewId, ReviewStatus status)
    {
        if (_reviews.TryGetValue(reviewId, out var request))
        {
            request.Status = status;
        }
        return Task.CompletedTask;
    }
}
```

### Part 4: Create Review API Endpoints

Build API endpoints for human reviewers to respond:

```csharp
// Program.cs
app.MapGet("/api/reviews/pending", async (IReviewQueueService queue) =>
{
    var reviews = await queue.GetPendingReviewsAsync();
    return Results.Ok(reviews);
});

app.MapGet("/api/reviews/{reviewId:guid}", async (
    Guid reviewId,
    IReviewQueueService queue) =>
{
    var review = await queue.GetPendingReviewAsync(reviewId);
    return review != null ? Results.Ok(review) : Results.NotFound();
});

app.MapPost("/api/reviews/{reviewId:guid}/respond", async (
    Guid reviewId,
    ReviewResponse response,
    IReviewQueueService queue,
    IProcessRuntime processRuntime) =>
{
    var review = await queue.GetPendingReviewAsync(reviewId);
    if (review == null)
        return Results.NotFound();
    
    // Update review status
    await queue.UpdateReviewStatusAsync(
        reviewId,
        response.Approved ? ReviewStatus.Approved : ReviewStatus.Rejected);
    
    // Resume the process with human decision
    var eventId = response.Approved 
        ? ProcessEvents.ReviewApproved 
        : ProcessEvents.ReviewRejected;
    
    var decision = new ReviewDecision
    {
        ReviewId = reviewId,
        Approved = response.Approved,
        Comments = response.Comments,
        ReviewedBy = response.ReviewedBy,
        ReviewedAt = DateTime.UtcNow,
        RequiredActions = response.RequiredActions
    };
    
    await processRuntime.SendEventAsync(review.ProcessId, eventId, decision);
    
    return Results.Ok(new { message = "Review response recorded" });
});
```

### Part 5: Implement Timeout and Escalation

Add timeout handling for pending reviews:

```csharp
public class ReviewTimeoutStep : KernelProcessStep
{
    private readonly IReviewQueueService _reviewQueue;
    private readonly INotificationService _notificationService;
    
    public static class Functions
    {
        public const string CheckTimeout = nameof(CheckTimeout);
    }

    [KernelFunction(Functions.CheckTimeout)]
    public async Task CheckTimeoutAsync(
        KernelProcessStepContext context,
        ReviewRequest request)
    {
        // Start timeout timer
        var timeoutTask = Task.Delay(request.ExpiresAt - DateTime.UtcNow);
        var reviewCompleteTask = WaitForReviewCompletionAsync(request.Id);
        
        var completedTask = await Task.WhenAny(timeoutTask, reviewCompleteTask);
        
        if (completedTask == timeoutTask)
        {
            // Timeout occurred - escalate
            await _notificationService.NotifyManagerAsync(new EscalationNotification
            {
                ReviewId = request.Id,
                Reason = "Review timeout - no response within 24 hours",
                EscalatedAt = DateTime.UtcNow
            });
            
            await context.EmitEventAsync(new KernelProcessEvent
            {
                Id = ProcessEvents.ReviewTimedOut,
                Data = request
            });
        }
    }
    
    private async Task WaitForReviewCompletionAsync(Guid reviewId)
    {
        // Poll for review completion
        while (true)
        {
            var review = await _reviewQueue.GetPendingReviewAsync(reviewId);
            if (review?.Status != ReviewStatus.Pending)
                break;
            
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
```

## Integration with .NET Aspire

Configure your Process Framework application with Aspire:

```csharp
// AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

var processOrchestrator = builder.AddProject<ProcessOrchestrator>("process-orchestrator")
    .WithHttpsEndpoint(port: 5100, name: "https");

var reviewService = builder.AddProject<ReviewService>("review-service")
    .WithReference(processOrchestrator)
    .WithHttpsEndpoint(port: 5101, name: "https");

builder.Build().Run();
```

Enable distributed tracing for process execution:

```csharp
// Program.cs
AppContext.SetSwitch(
    "Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", 
    true);
```

## Testing Your Implementation

### 1. Test Process Execution

```csharp
var code = @"
public class UserService
{
    public string GetUserPassword(int userId)
    {
        return database.Query($""SELECT password FROM users WHERE id={userId}"");
    }
}";

var process = await CodeReviewProcess.BuildProcessAsync(kernel);

await using var runningProcess = await process.StartAsync(
    kernel,
    new KernelProcessEvent
    {
        Id = ProcessEvents.StartReview,
        Data = code
    });

// Process will pause at human review step
// Test responding via API
```

### 2. Test Human Review API

```http
### Get Pending Reviews
GET https://localhost:5101/api/reviews/pending

### Get Specific Review
GET https://localhost:5101/api/reviews/{{reviewId}}

### Approve Review
POST https://localhost:5101/api/reviews/{{reviewId}}/respond
Content-Type: application/json

{
  "approved": true,
  "comments": "Looks good, approved",
  "reviewedBy": "senior.dev@company.com"
}

### Reject Review
POST https://localhost:5101/api/reviews/{{reviewId}}/respond
Content-Type: application/json

{
  "approved": false,
  "comments": "SQL injection vulnerability must be fixed",
  "reviewedBy": "security.lead@company.com",
  "requiredActions": [
    "Use parameterized queries",
    "Implement input validation",
    "Add logging"
  ]
}
```

### 3. Monitor in Aspire Dashboard

- View process execution traces
- Monitor step completion times
- Track event flows
- See HITL pause points

## Deliverables

1. **Complete Process Implementation**
   - All process steps implemented
   - Event flow properly wired
   - HITL integration working

2. **Review Queue Service**
   - Store and manage review requests
   - Track review status
   - Handle concurrent reviews

3. **Review API**
   - Endpoints for listing reviews
   - Endpoints for responding to reviews
   - Process resume on human response

4. **Timeout & Escalation**
   - Timeout detection
   - Manager escalation
   - Fallback strategies

5. **Aspire Integration**
   - All services in Aspire
   - Distributed tracing enabled
   - Service discovery configured

6. **Testing Documentation**
   - Process execution examples
   - API testing examples
   - Screenshots from Aspire dashboard

## Advanced Challenges (Optional)

1. **Multi-Level Approval**
   - Require multiple approvers for critical changes
   - Escalation hierarchy (developer → lead → manager)

2. **Conditional Branching**
   - Different flows based on severity
   - Automatic approval for low-risk changes

3. **Audit Trail**
   - Log all decisions
   - Track who approved what and when
   - Generate compliance reports

4. **SLA Management**
   - Different timeouts based on priority
   - Automated actions on SLA breach
   - Performance metrics

## Tips

- Start with a simple linear process before adding branches
- Test HITL steps thoroughly - they're critical for production
- Use Aspire dashboard to visualize process flow
- Log every event for debugging
- Handle process failures gracefully
- Consider data persistence for long-running processes

## Resources

- [Notebook 7: Process Framework & HITL](../../notebooks/7-Process-Framework-and-HITL.ipynb)
- [Process Framework with Aspire Sample](https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/Demos/ProcessFrameworkWithAspire)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)

## Success Criteria

- ✅ Process executes all steps in correct order
- ✅ Human review step pauses process correctly
- ✅ API endpoints allow reviewers to respond
- ✅ Process resumes after human decision
- ✅ Both approval and rejection flows work
- ✅ Timeout and escalation implemented
- ✅ Distributed tracing visible in Aspire
- ✅ Proper error handling throughout

This assignment demonstrates production-ready AI workflows with human oversight - essential for building trustworthy, compliant AI systems.