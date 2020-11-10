namespace Moonrise.Utils.Standard.CSV
{
    /// <summary>
    ///     Used to convert Csv input strings to whatever your heart desires to be stored in your object.
    /// </summary>
    public interface ICsvConverter
    {
        /// <summary>
        ///     Converts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Your dream</returns>
        object Convert(string input);
    }
}
