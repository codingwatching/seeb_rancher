using System.Collections.Generic;

namespace Genetics.GeneticDrivers
{
    public class CompiledGeneticDrivers
    {
        private Dictionary<string, object> geneticDriverValues = new Dictionary<string, object>();

        private bool writable = true;
        /// <summary>
        /// Lock the genetic driver set, indicating that it is fully compiled and cannot be changed
        /// </summary>
        /// <returns>true if it became locked, false if the drivers were already locked</returns>
        public bool Lock()
        {
            return writable && !(writable = false);
        }

        public bool TryGetGeneticData<T>(GeneticDriver<T> driver, out T driverValue)
        {
            if (geneticDriverValues.TryGetValue(driver.DriverName, out var objectValue) && objectValue is T typedValue)
            {
                driverValue = typedValue;
                return true;
            }
            driverValue = default;
            return false;
        }

        public void SetGeneticDriverData<T>(GeneticDriver<T> driver, T value)
        {
            if (!writable)
            {
                return;
            }
            geneticDriverValues[driver.DriverName] = value;
        }
    }
}
