﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

public class UCHHud : RootPanel
{
	public static UCHHud CurHud;

	public UCHHud()
	{
		if (CurHud != null)
		{
			CurHud.Delete();
			CurHud = null;
		}

		//Temporary
		AddChild<ChatBox>();
		AddChild<Scoreboard<ScoreboardEntry>>();
		AddChild<VoiceList>();
		AddChild<VoiceSpeaker>();

		AddChild<RoundTimer>();

		CurHud = this;
	}
}

