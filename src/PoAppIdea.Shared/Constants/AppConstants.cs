namespace PoAppIdea.Shared.Constants;

/// <summary>
/// Application-wide constants.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Application name.
    /// </summary>
    public const string AppName = "PoAppIdea";

    /// <summary>
    /// Number of ideas per batch in Phase 1.
    /// </summary>
    public const int IdeasPerBatch = 10;

    /// <summary>
    /// Total number of batches in Phase 1.
    /// </summary>
    public const int TotalBatches = 5;

    /// <summary>
    /// Total ideas generated in Phase 1 (50).
    /// </summary>
    public const int TotalIdeasPhase1 = IdeasPerBatch * TotalBatches;

    /// <summary>
    /// Number of top ideas selected after Phase 1.
    /// </summary>
    public const int TopIdeasAfterPhase1 = 2;

    /// <summary>
    /// Number of mutations per top idea in Phase 2.
    /// </summary>
    public const int MutationsPerIdea = 1;

    /// <summary>
    /// Number of top evolved ideas after Phase 2.
    /// </summary>
    public const int TopIdeasAfterPhase2 = 5;

    /// <summary>
    /// Number of feature variations per evolved idea in Phase 3.
    /// </summary>
    public const int FeatureVariationsPerIdea = 5;

    /// <summary>
    /// Maximum number of ideas that can be selected for synthesis.
    /// </summary>
    public const int MaxSelectableIdeas = 10;

    /// <summary>
    /// Number of questions in Phase 4 (PM) and Phase 5 (Architect).
    /// </summary>
    public const int QuestionsPerRefinementPhase = 5;

    /// <summary>
    /// Number of visual assets generated in Phase 6.
    /// </summary>
    public const int VisualAssetsGenerated = 10;

    /// <summary>
    /// Swipe speed thresholds in milliseconds.
    /// </summary>
    public static class SwipeThresholds
    {
        /// <summary>
        /// Fast swipe threshold (&lt;1000ms).
        /// </summary>
        public const int FastMaxMs = 1000;

        /// <summary>
        /// Slow swipe threshold (&gt;3000ms).
        /// </summary>
        public const int SlowMinMs = 3000;
    }

    /// <summary>
    /// Swipe weight multipliers.
    /// </summary>
    public static class SwipeWeights
    {
        /// <summary>
        /// Fast + Like = 2x weight.
        /// </summary>
        public const float FastLike = 2.0f;

        /// <summary>
        /// Medium + Like = 1x weight.
        /// </summary>
        public const float MediumLike = 1.0f;

        /// <summary>
        /// Slow + Like = 0.5x weight.
        /// </summary>
        public const float SlowLike = 0.5f;
    }

    /// <summary>
    /// Validation constraints.
    /// </summary>
    public static class Validation
    {
        public const int MinComplexityLevel = 1;
        public const int MaxComplexityLevel = 5;
        public const int MaxTitleLength = 100;
        public const int MaxDescriptionLength = 500;
        public const int MaxMergedDescriptionLength = 1000;
        public const int MaxAnswerLength = 2000;
        public const int MaxDislikedPatterns = 50;
        public const int MinSlugLength = 5;
        public const int MaxSlugLength = 100;
    }

    /// <summary>
    /// Azure Table Storage table names.
    /// </summary>
    public static class TableNames
    {
        public const string Users = "Users";
        public const string Personalities = "Personalities";
        public const string Sessions = "Sessions";
        public const string Ideas = "Ideas";
        public const string Swipes = "Swipes";
        public const string Mutations = "Mutations";
        public const string FeatureVariations = "FeatureVariations";
        public const string Syntheses = "Syntheses";
        public const string RefinementAnswers = "RefinementAnswers";
        public const string VisualAssets = "VisualAssets";
        public const string Artifacts = "Artifacts";
    }

    /// <summary>
    /// Azure Blob Storage container names.
    /// </summary>
    public static class ContainerNames
    {
        public const string VisualAssets = "visual-assets";
        public const string ArtifactPacks = "artifact-packs";
    }
}
