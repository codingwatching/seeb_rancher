using Assets.Scripts.DataModels;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    public interface ISeedHoldingManipulator
    {
        bool AttemptTransferAllSeedsInto(SeedBucket target);
        SeedBucketUI SwapSeedsWithBucket(SeedBucketUI target);
        int PlantIdOfSeebs();
        string SeedGroupName { get; }
        /// <summary>
        /// will either take exactly <paramref name="seedCount"/> seeds, or take zero seeds.
        /// </summary>
        /// <param name="seedCount">The number of seeds to take</param>
        /// <returns>A seed array of length <paramref name="seedCount"/>, or null</returns>
        Seed[] AttemptTakeSeeds(int seedCount);
    }
}
