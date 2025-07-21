using System.Collections.Generic;
using System.IO;

namespace ProcessorEmulator.Tools
{
    /// <summary>
    /// Emulates a satellite LNB with Single-Wire Multiswitch band stacking.
    /// </summary>
    public interface ISatelliteLnbEmulator
    {
        /// <summary>Initialize with number of user bands, band frequency map, and optional feed URL.</summary>
        bool Initialize(int userBandCount, IDictionary<int, int> bandFrequencies, string feedUrl);
        /// <summary>Select horizontal (true) or vertical (false) polarization.</summary>
        void SetPolarization(bool horizontal);
        /// <summary>Select the active user band by index (1-based).</summary>
        void SelectUserBand(int bandIndex);
        /// <summary>Get the IF center frequency for the current band.</summary>
        int GetCurrentIf();
        /// <summary>Get a stream of the satellite signal (from URL or stub).</summary>
        Stream GetSignalStream();
    }
}
