namespace HSPGUI
{
    /// <summary>
    /// The Constants class holds constant values used throughout the HSPGUI application.
    /// These constants include default IP address, port, and various configuration settings.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// The key for storing the IP address in preferences.
        /// </summary>
        public const string KeyIpAddress = "IpAddress";

        /// <summary>
        /// The default IP address for the HSP server.
        /// </summary>
        public const string IpAddress = "192.168.50.124";

        /// <summary>
        /// The key for storing the port in preferences.
        /// </summary>
        public const string KeyPort = "Port";

        /// <summary>
        /// The default port for the HSP server.
        /// </summary>
        public const int Port = 5000;
        /// <summary>
        /// The key for storing the port in preferences.
        /// </summary>
        public const string KeyPortDATA = "PortDATA";

        /// <summary>
        /// The default port for the HSP server.
        /// </summary>
        public const int PortDATA = 5001;
        /// <summary>
        /// The key for storing the port in preferences.
        /// </summary>
        public const string KeyPortDIAG= "PortDIAG";
        /// <summary>
        /// The default port for the HSP server.
        /// </summary>
        public const int PortDIAG = 5003;
        /// <summary>
        /// The maximum number of hexadecimal digits for the EPC (Electronic Product Code).
        /// </summary>
        public const int MaxLenEPC_hex = 32;

        /// <summary>
        /// The maximum number of hexadecimal digits for the User Data.
        /// </summary>
        public const int MaxLenUSR_Hex = 32;

        /// <summary>
        /// The scale factor used to convert font size to width.
        /// </summary>
        public const double fontToWidthScale = 0.65;
    }
}
