using System;

namespace Paraesthesia.EmbeddedPageHandlerFactory.Demo
{
	/// <summary>
	/// Web form that is not embedded in the assembly.
	/// </summary>
	public class NotEmbedded : System.Web.UI.Page
	{
		protected System.Web.UI.WebControls.Literal litDateTime;
	
		private void Page_Load(object sender, System.EventArgs e)
		{
			this.litDateTime.Text = DateTime.Now.ToString("r");
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion
	}
}
