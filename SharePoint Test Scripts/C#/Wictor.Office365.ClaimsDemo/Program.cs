using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using Microsoft.SharePoint.Client;


using SP = Microsoft.SharePoint.Client;

namespace Wictor.Office365.ClaimsDemo {

    public class FunnelbackXmlConfig
    {
        public string outputFolder { get; set; }
        public string targetSite { get; set; }
        public string[] WantedFields { get; set; }
        public string[] CDataFields { get; set; }
        public string[] LookupFields { get; set; }
        public string[] UserFields { get; set; }
    }

    public class FunnelbackXmlSite
    {
        public FunnelbackXmlConfig myfbx { get; set; }
        public Web ww { get; set; }

        public void Process()
        {
            if (this.ww != null)
            {
                WebCollection oWebs = this.ww.Webs;
                this.ww.Context.Load(oWebs);
                this.ww.Context.ExecuteQuery();
                foreach (Web sww in oWebs)
                {
                    Console.WriteLine("Site: {0}", sww.Title);
                    Console.ReadLine();
                    FunnelbackXmlSite fbxs = new FunnelbackXmlSite();
                    fbxs.myfbx = this.myfbx;
                    fbxs.ww = sww;
                    fbxs.Process();
                }
            }
        }

        public void FunnelbackWriteXml()
        {

        }
    }




 public class FunnelbackXmlRecord
	{
		public FunnelbackXmlConfig myfbx { get; set; }
		public ListItem li { get; set; }
				
		public string SafeFieldValue(string key)
		{
			string oSafeValueString = "None";
			if (this.li.FieldValues.Keys.Contains(key))
			{
				if (this.myfbx.WantedFields.Contains(key))
				{
					oSafeValueString = this.li.FieldValues[key].ToString();
				}
				if (this.myfbx.CDataFields.Contains(key))
				{
					oSafeValueString = @"<![CDATA[" + oSafeValueString + @"]]>";
				}
				if (this.myfbx.LookupFields.Contains(key))
				{
					FieldUserValue oFLV = (FieldUserValue)this.li.FieldValues[key];
					oSafeValueString = oFLV.LookupValue;
				}
			}
			return oSafeValueString;		
		}
		
		public void FunnelbackWriteXml()
		{
			if (this.li != null)
			{
				using (StreamWriter writer = new StreamWriter(this.myfbx.outputFolder + @"\" + this.li["UniqueId"].ToString() + ".xml"))
				{
					writer.WriteLine(@"<?xml version='1.0'?>");
					writer.WriteLine(@"<FBSPRecord>");
					writer.WriteLine("<id>{0}</id><title>{1}</title><hura>{2}</hura><type>{3}</type><null>{4}</null>",
                    	                  this.li.Id,
                    	                  this.li.DisplayName,
                    	                  this.li.HasUniqueRoleAssignments,
                    	                  this.li.FileSystemObjectType,
                    	                  this.li.ServerObjectIsNull
                    	                 );                 			
					List<string> klist = new List<string>(this.li.FieldValues.Keys);
					foreach (String klistkey in klist)
					{
						string oSFV = SafeFieldValue(klistkey);
						if (oSFV != "None")
						{
							writer.WriteLine("<{0}>{1}</{0}>", klistkey.Replace(" ", "_"), oSFV);							
						}
					}
					writer.WriteLine(@"</FBSPRecord>");
				}				
			}
		}
	}
    
    class Program {
       

        static void Main(string[] args) {
            //lets create config file for funnelback and read it first
            Dictionary<string, string> fnb_config = new Dictionary<string, string>();
            
            try
            { 
                using (StreamReader sr = new StreamReader("funnelback.cfg"))
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
           // wanted_fields=new string[] {""};
            if (fnb_config.ContainsKey("wanted_fields"))
            {
                string wanted_fields_string = fnb_config["wanted_fields"];
                 wanted_fields = wanted_fields_string.Split(',');

            }
            string[] cdata_fields = { "" };
            if (fnb_config.ContainsKey("cdata_fields"))
            {
                string cdata_fields_string = fnb_config["cdata_fields"];
                wanted_fields = cdata_fields_string.Split(',');

            }
            string[] lookup_fields = { "" };
            if (fnb_config.ContainsKey("lookup_fields"))
            {
                string lookup_fields_string = fnb_config["lookup_fields"];
                wanted_fields = lookup_fields_string.Split(',');

            }
            
            string target_site = fnb_config["target_site"];
            string username = fnb_config["username"];
            string password = fnb_config["password"];
            string output_folder = fnb_config["output_folder"];
            string auth_method = fnb_config["auth_method"];
 
            FunnelbackXmlConfig fbx = new FunnelbackXmlConfig();
       
            fbx.outputFolder = output_folder;
            fbx.targetSite = target_site;
            fbx.WantedFields = wanted_fields;
            fbx.CDataFields = cdata_fields;
            fbx.LookupFields = lookup_fields;
               



            //get all we need for claims authentication

            MsOnlineClaimsHelper claimsHelper = new MsOnlineClaimsHelper(target_site,username,password);
             
            //from now on we can use sharepoint being authenticated 
            using (ClientContext ctx = new ClientContext(target_site))
            {
                ctx.ExecutingWebRequest += claimsHelper.clientContext_ExecutingWebRequest;

                ctx.Load(ctx.Web);
                ctx.ExecuteQuery();
                if (ctx != null)
                {
                    using (StreamWriter writer = new StreamWriter(fbx.outputFolder + "\\first.xml"))
                    {
                        Site oSite = ctx.Site;
                        WebCollection oWebs = oSite.RootWeb.Webs;
                        FunnelbackXmlSite fbxs = new FunnelbackXmlSite();
                        fbxs.ww = oSite.RootWeb;
                        fbxs.myfbx = fbx;
                        fbxs.Process();
                        ctx.Load(oWebs);
                        ctx.ExecuteQuery();
                        writer.WriteLine(@"<?xml version='1.0'?>");
                        writer.WriteLine(@"<sharepoint>");
                        foreach (Web oWebsite in oWebs)
                        {

                            ListCollection collList = oWebsite.Lists;
                            ctx.Load(collList); // Query for Web
                            ctx.ExecuteQuery(); // Execute

                            writer.WriteLine(@"<site>");
                            writer.WriteLine("<title>{0}</title>", oWebsite.Title);
                            foreach (List oList in collList)
                            {
                                writer.WriteLine("<list>{0}</list>", oList.Title);
                                List oListy = collList.GetByTitle(oList.Title);
                                CamlQuery camlQuery = new CamlQuery();
                                camlQuery.ViewXml = "<View><RowLimit>100</RowLimit></View>";
                                ListItemCollection collListItem = oListy.GetItems(camlQuery);
                                ctx.Load(collListItem,
                                         items => items.IncludeWithDefaultProperties(
                                            item => item.DisplayName,
                                            item => item.HasUniqueRoleAssignments
                                         ));
                                ctx.ExecuteQuery();
                                foreach (ListItem oListItem in collListItem)
                                {
                                    FunnelbackXmlRecord oFXR = new FunnelbackXmlRecord();
                                    oFXR.myfbx = fbx;
                                    oFXR.li = oListItem;
                                    oFXR.FunnelbackWriteXml();
                                }
                            }
                            writer.WriteLine(@"</site>");
                        }
                        writer.WriteLine(@"</sharepoint>");

                    }
                }
                

            }
        }
    }
}
