using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class TeamHud : Panel
{
	public Panel MainPanel;

	public TeamHud()
	{
		StyleSheet.Load( "ui/teamhud.scss" );
	}
}

