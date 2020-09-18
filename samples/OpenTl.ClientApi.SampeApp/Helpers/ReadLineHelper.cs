namespace OpenTl.ClientApi.SampeApp.Helpers
{
    using System;

    public static class ReadLineHelper
    {
        public static string Read(string promt)
        {
            var line = ReadInternal(promt);

            ReadLine.AddHistory(line);

            return line;
        }

        private static string ReadInternal(string promt, bool passwordMode = false)
        {
            string line;
            do
            {
                line = passwordMode ? ReadLine.ReadPassword(promt) : ReadLine.Read(promt);
            }
            while (string.IsNullOrWhiteSpace(line));

            return line;
        }

        public static string ReadPassword(string promt)
        {
            var line = ReadInternal(promt, true);

            return line;
        }
    }
}