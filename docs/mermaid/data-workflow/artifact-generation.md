# Data Workflow - Artifact Generation

Sequence diagram showing how final artifacts are generated.

```mermaid
sequenceDiagram
    autonumber
    participant User
    participant ArtifactsPage as ArtifactsPage.razor
    participant ArtifactSvc as ArtifactService
    participant ArtifactGen as ArtifactGenerator
    participant OpenAI as Azure OpenAI
    participant BlobStorage as Blob Storage
    participant TableStorage as Table Storage

    User->>ArtifactsPage: Click "Generate Artifacts"
    ArtifactsPage->>ArtifactSvc: GenerateArtifactsAsync(sessionId)
    
    ArtifactSvc->>TableStorage: GetSessionWithAllDataAsync
    Note over ArtifactSvc,TableStorage: Fetches: Session, Synthesis,<br/>RefinementAnswers, SelectedVisual
    TableStorage-->>ArtifactSvc: Complete session data
    
    par Generate PRD
        ArtifactSvc->>ArtifactGen: GeneratePRDAsync(context)
        ArtifactGen->>OpenAI: ChatCompletionAsync(PRD prompt)
        OpenAI-->>ArtifactGen: PRD markdown
        ArtifactGen-->>ArtifactSvc: PRD content
    and Generate Tech Deep-Dive
        ArtifactSvc->>ArtifactGen: GenerateTechDocAsync(context)
        ArtifactGen->>OpenAI: ChatCompletionAsync(Tech prompt)
        OpenAI-->>ArtifactGen: Tech doc markdown
        ArtifactGen-->>ArtifactSvc: Tech content
    end
    
    loop For each artifact type
        ArtifactSvc->>ArtifactSvc: Create Artifact entity
        ArtifactSvc->>BlobStorage: UploadAsync(content)
        BlobStorage-->>ArtifactSvc: Blob URL
        ArtifactSvc->>TableStorage: UpsertEntityAsync<Artifact>
        TableStorage-->>ArtifactSvc: Success
    end
    
    ArtifactSvc->>ArtifactSvc: Create download package
    ArtifactSvc->>BlobStorage: UploadZipAsync(package)
    BlobStorage-->>ArtifactSvc: Package URL
    
    ArtifactSvc-->>ArtifactsPage: ArtifactGenerationResult
    ArtifactsPage-->>User: Display artifacts with download links
```

## Artifact Types

| Type | Description | Format |
|------|-------------|--------|
| **PRD** | Product Requirements Document | Markdown |
| **TechDeepDive** | Technical Architecture Document | Markdown |
| **VisualPack** | Selected mockups and style guide | Images + JSON |
| **DownloadPackage** | All artifacts bundled | ZIP |

## Context Aggregation

The artifact generator receives a complete context object:

```json
{
  "session": { "appType": "Mobile", "complexity": 4 },
  "synthesis": { "mergedConcept": "...", "thematicBridge": "..." },
  "pmAnswers": [ { "question": "...", "answer": "..." } ],
  "techAnswers": [ { "question": "...", "answer": "..." } ],
  "selectedVisual": { "style": "...", "palette": "..." }
}
```
