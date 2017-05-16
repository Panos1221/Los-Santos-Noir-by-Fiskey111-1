﻿using Gwen.Control;
using Rage;
using System.Threading;

namespace LSNoir.Computer.GwenForms
{
    class Progress : Rage.Forms.GwenForm
    {
        private Label sending;
        private ProgressBar progressBar;

        private readonly string labTxt;

        public Progress(string labelText) : base(typeof(WinForms.Progress))
        {
            labTxt = labelText;
        }

        public override void InitializeLayout()
        {
            int x = (Game.Resolution.Width / 2) - (Size.Width / 2);
            int y = (Game.Resolution.Height / 2) - (Size.Height / 2);

            Position = new System.Drawing.Point(x, y);

            sending.Alignment = Gwen.Pos.CenterH;
            progressBar.Alignment = Gwen.Pos.CenterH;

            GameFiber.StartNew(delegate
            {
                //TODO: use timer to change text
                var txt = 0f;
                for (var i = 0f; i < 1f; i = i + MathHelper.GetRandomSingle(0.01f, 0.10f))
                {
                    progressBar.Value = i;

                    float remainder = txt % 4;
                    string text = labTxt;

                    if (remainder == 0.25f) text = labTxt + ".";
                    else if (remainder == 0.50f) text = labTxt + "..";
                    else if (remainder == 0.75f) text = labTxt + "...";

                    sending.Text = text;

                    txt++;

                    GameFiber.Sleep(MathHelper.GetRandomInteger(0500, 1000));
                }


                Window.Close();
            });

            base.InitializeLayout();
        }
    }
}
