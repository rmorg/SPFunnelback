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
	}
	
	public class FunnelbackXmlExporter
	{
		[STAThread]
		static void Main()
		{
			FunnelbackXmlConfig fbx = new FunnelbackXmlConfig();
			fbx.outputFolder = @"C:\Users\rpfmorg\output";
			fbx.targetSite = @"http://funnelback.sharepoint.com/teamsite/";
			
			using (ClientContext ctx = ClaimClientContext.GetAuthenticatedContext(fbx.targetSite))
			{
				if (ctx != null)
				{
					using (StreamWriter writer = new StreamWriter(fbx.outputFolder + "\\first.xml"))
					{
						Web oWebsite = ctx.Web;
						ListCollection collList = oWebsite.Lists;
						ctx.Load(collList); // Query for Web
						ctx.ExecuteQuery(); // Execute
						writer.WriteLine(@"<?xml version='1.0'?>");
						writer.WriteLine(@"<sharepoint>");
						foreach (SP.List oList in collList)
						{
							writer.WriteLine("<title>{0}</title>", oList.Title);
							SP.List oListy = ctx.Web.Lists.GetByTitle(oList.Title);
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
                    			writer.WriteLine("<record>");
                    			writer.WriteLine("<id>{0}</id><title>{1}</title><hura>{2}</hura><type>{3}</type><null>{4}</null>",
                    	                  oListItem.Id,
                    	                  oListItem.DisplayName,
                    	                  oListItem.HasUniqueRoleAssignments,
                    	                  oListItem.FileSystemObjectType,
                    	                  oListItem.ServerObjectIsNull
                    	                 );                 			
                    			List<string> klist = new List<string>(oListItem.FieldValuesAsText.FieldValues.Keys);
                    			foreach (String klistkey in klist)
                    			{
                    				writer.WriteLine("<{0}><!CDATA[{1}]]></{0}>", klistkey.Replace(" ", "_"), oListItem.FieldValuesAsText.FieldValues[klistkey]);
                    			}
                    			writer.WriteLine("</record>");
                    		}
                  		
                    }
						writer.WriteLine(@"</sharepoint>");
					}
				}
			}
		}
	}
	
}