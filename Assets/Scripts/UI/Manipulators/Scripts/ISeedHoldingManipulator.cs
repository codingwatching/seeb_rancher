using Simulation.Plants.PlantData;
using UI.SeedInventory;

namespace UI.Manipulators
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
