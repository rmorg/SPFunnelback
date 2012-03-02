/*
 * Created by SharpDevelop.
 * User: rpfmorg
 * Date: 01/03/2012
 * Time: 15:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.SharePoint.Client;

namespace FUNN_SP_PROXIES
{
	/// <summary>
	/// Funnelback proxy for a Sharepoint Site
	/// </summary>
	[XmlRoot("fbSite")]
	public class FunnelbackSite
	{
		public FunnelbackConfig myfbx { get; set; }
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
                    FunnelbackSite fbxs = new FunnelbackSite();
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
}
