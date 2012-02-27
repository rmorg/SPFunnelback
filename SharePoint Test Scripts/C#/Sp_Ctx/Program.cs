using System;
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
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // if (args.Length < 1) { Console.WriteLine("SP_Ctx <url>"); return; }

            string targetSite = "http://funnelback.sharepoint.com/teamsite/";
            using (ClientContext ctx = ClaimClientContext.GetAuthenticatedContext(targetSite))
            {
                if (ctx != null)
                {
                	Web oWebsite = ctx.Web;
                	ListCollection collList = oWebsite.Lists;
                    ctx.Load(collList); // Query for Web
                    ctx.ExecuteQuery(); // Execute
                    foreach (SP.List oList in collList)
                    {
                    	Console.WriteLine("Title: {0} Created: {1}", oList.Title, oList.Created.ToString());
                    }
                    SP.List oListy = ctx.Web.Lists.GetByTitle("Site Pages");
                    
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = "<View><RowLimit>100</RowLimit></View>";
                    
                    ListItemCollection collListItem = oListy.GetItems(camlQuery);
                    ctx.Load(collListItem,
                             items => items.IncludeWithDefaultProperties(
                             	item => item.DisplayName,
                             	item => item.HasUniqueRoleAssignments));
                    ctx.ExecuteQuery();
                    foreach (ListItem oListItem in collListItem)
                    {
                    	Console.WriteLine("Id: {0} DisplayName: {1} HURA: {2} Type: {3} Null: {4}", 
                    	                  oListItem.Id,
                    	                  oListItem.DisplayName,
                    	                  oListItem.HasUniqueRoleAssignments,
                    	                  oListItem.FileSystemObjectType,
                    	                  oListItem.ServerObjectIsNull
                    	                 );
                    	File oFile = oListItem.File;
                    	if (oListItem.ServerObjectIsNull == false)
                    	{
	                    	LimitedWebPartManager lwpm = oFile.GetLimitedWebPartManager(PersonalizationScope.Shared);
                    		ctx.Load(lwpm.WebParts,
	                    	         wps => wps.IncludeWithDefaultProperties(
	                    	         	wp => wp.WebPart.Title
	                    	         ));
	                    	ctx.ExecuteQuery();
	                    	Console.WriteLine("Number of WebParts: {0}", lwpm.WebParts.Count.ToString());
	                    	if (lwpm.WebParts.Count > 0 )
	                    	{
	                    		foreach (WebPartDefinition wp in lwpm.WebParts)
	                    		{
	                    			Console.WriteLine("WebPart Title: {0}", wp.WebPart.Title);
	                    		}
	                    	}
                    	}
                    	List<string> klist = new List<string>(oListItem.FieldValues.Keys);
                    	foreach (String klistkey in klist)
                    	{
                    		Console.WriteLine("Key: {0} Value: {1}", klistkey, oListItem.FieldValues[klistkey]);
                    	}
                  		
                    }
                }
            }
            Console.ReadLine();
        }
    }
}
