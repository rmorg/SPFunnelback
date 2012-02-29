/*
 * Created by SharpDevelop.
 * User: rpfmorg
 * Date: 27/02/2012
 * Time: 11:00
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using System.Net;
using MSDN.Samples.ClaimsAuth;
using SP = Microsoft.SharePoint.Client;

namespace Sp_Ctx
{
	public class FunnelbackXmlConfig
	{
		public string outputFolder { get; set; }
		public string targetSite { get; set; }
		public string[] WantedFields { get; set; }
		public string[] CDataFields { get; set; }
		public string[] LookupFields { get; set; }
		public string[] UserFields { get; set; }
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
	
	public class FunnelbackXmlExporter
	{
		[STAThread]
		static void Main()
		{
			FunnelbackXmlConfig fbx = new FunnelbackXmlConfig();
			fbx.outputFolder = @"C:\Users\rpfmorg\output";
			fbx.targetSite = @"http://funnelback.sharepoint.com/teamsite/";
			fbx.WantedFields = new string[] {"WikiField", "FileRef", "FileDirRef", "FileLeafRef", "Created", "Modified"};
			fbx.CDataFields =  new string[] {"WikiField"};
			fbx.LookupFields = new string[] {"Author"};
			
			using (ClientContext ctx = ClaimClientContext.GetAuthenticatedContext(fbx.targetSite))
			{
				if (ctx != null)
				{
					using (StreamWriter writer = new StreamWriter(fbx.outputFolder + "\\first.xml"))
					{
						Site oSite = ctx.Site;
						WebCollection oWebs = oSite.RootWeb.Webs;
						ctx.Load(oWebs);
						ctx.ExecuteQuery();
						writer.WriteLine(@"<?xml version='1.0'?>");
						writer.WriteLine(@"<sharepoint>");
						foreach(Web oWebsite in oWebs)
						{
										
							ListCollection collList = oWebsite.Lists;
							ctx.Load(collList); // Query for Web
							ctx.ExecuteQuery(); // Execute
							
							writer.WriteLine(@"<site>");
							foreach (List oList in collList)
							{
								writer.WriteLine("<title>{0}</title>", oList.Title);
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