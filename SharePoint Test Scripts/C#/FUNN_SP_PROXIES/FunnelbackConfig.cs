/*
 * Created by SharpDevelop.
 * User: rpfmorg
 * Date: 01/03/2012
 * Time: 16:28
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.SharePoint.Client;

namespace FUNN_SP_PROXIES
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public class FunnelbackConfig
	{
		#region Properties
		public string outputFolder { get; set; }
        public string targetSite { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string authMethod { get; set; }
        public string[] WantedFields { get; set; }
        public string[] CDataFields { get; set; }
        public string[] LookupFields { get; set; }
        public string[] UserFields { get; set; }
		#endregion
        
		#region Constructor
		public FunnelbackConfig(string configfilepath)
		{
			//lets create config file for funnelback and read it first
            Dictionary<string, string> fnb_config = new Dictionary<string, string>();
            
            try
            { 
                using (StreamReader sr = new StreamReader(configfilepath))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                       // Console.WriteLine(line);
                        //Dictionary<string, string> fnb_dictionary = new Dictionary<string, string>();
                        string[] words = line.Split('=');
                        fnb_config.Add(words[0],words[1]);
                       // Console.WriteLine(words[0] + words[1]);
                    }
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
           // Console.WriteLine(fnb_config);

            foreach (KeyValuePair<string, string> pair in fnb_config)
            {
                Console.WriteLine("{0}, {1}",
                pair.Key,
                pair.Value);
            }

            string[] wanted_fields = { "" };
            if (fnb_config.ContainsKey("wanted_fields"))
            {
                string wanted_fields_string = fnb_config["wanted_fields"];
                this.WantedFields = wanted_fields_string.Split(',');

            }
            string[] cdata_fields = { "" };
            if (fnb_config.ContainsKey("cdata_fields"))
            {
                string cdata_fields_string = fnb_config["cdata_fields"];
                this.CDataFields = cdata_fields_string.Split(',');

            }
            string[] lookup_fields = { "" };
            if (fnb_config.ContainsKey("lookup_fields"))
            {
                string lookup_fields_string = fnb_config["lookup_fields"];
                this.LookupFields = lookup_fields_string.Split(',');

            }
            
            this.targetSite = fnb_config["target_site"];
            this.username = fnb_config["username"];
            this.password = fnb_config["password"];
            this.outputFolder = fnb_config["output_folder"];
            this.authMethod = fnb_config["auth_method"];
		}
		#endregion
 	}
}