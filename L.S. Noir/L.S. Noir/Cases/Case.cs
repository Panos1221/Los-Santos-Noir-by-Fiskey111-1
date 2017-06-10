﻿using LSNoir.Data;
using LSNoir.DataAccess;
using LSNoir.Settings;
using LtFlash.Common.ScriptManager.Managers;
using LtFlash.Common.ScriptManager.Scripts;
using LtFlash.Common.Serialization;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSNoir.Cases
{
    class Case : BasicScript
    {
        //TODO:
        // - start all cases in stageData.NextStages

        //NOTES:
        // - now handles only one-by-one order of execution
        
        private readonly CaseData data;
        private CaseProgress Progress => data.GetCaseProgress();

        private readonly List<StageData> stages = new List<StageData>();
        private readonly AdvancedScriptManager manager = new AdvancedScriptManager();

        private const string NAMESPACE_STAGES = "LSNoir.Stages";

        public Case(CaseData cd)
        {
            data = cd;

            StageData[] stagesData = data.GetAllStagesData();
        
            Array.ForEach(stagesData, s => s.ParentCase = data);

            stages.AddRange(stagesData);
        }

        private static Type GetStageTypeByName(string name)
        {
            return Type.GetType($"{NAMESPACE_STAGES}.{name}", true, true);
        }

        private static void RegisterStages(AdvancedScriptManager mgr, ICollection<StageData> data)
        {
            foreach (var s in data)
            {
                var type = GetStageTypeByName(s.StageType);
                var ctorParams = new object[] { s };
                var prior = s.FinishPriorThis ?? new List<List<string>>();
                var next = s.NextScripts?.ToList() ?? new List<string>();
                var delayMin = s.DelayMinSeconds * 1000;
                var delayMax = s.DelayMaxSeconds * 1000;

                mgr.AddScript(type, ctorParams, s.ID, EInitModels.TimerBased, next, prior, delayMin, delayMax);
            }
        }

        protected override bool Initialize()
        {
            RegisterStages(manager, stages);

            //if this case was already finished before start it from the beginning
            var caseProgress = data.GetCaseProgress();

            Game.LogTrivial("Finished status: " + caseProgress.Finished);

            if(caseProgress.Finished)
            {
                caseProgress = new CaseProgress();
                Serializer.SaveItemToXML(caseProgress, data.CaseProgressPath);
            }

            if (!string.IsNullOrEmpty(caseProgress.LastStageID))
            {
                //TODO: use CaseProgress.NextScripts?
                var next = GetNextScriptID(data.Stages, caseProgress.LastStageID);

                manager.StartScript(data.Stages[next]);
                Game.LogTrivial("Start script: " + data.Stages[next]);
            }
            else
            {
                Game.LogTrivial("Start script: first");

                manager.Start();
            }

            return true;
        }

        private static int GetNextScriptID(string[] stages, string lastStage)
        {
            var id = stages.ToList().IndexOf(lastStage) + 1;

            if (id >= stages.Length)
            {
                var msg = $"{nameof(Case)}.{nameof(Initialize)}: next script id is out of range: {id}/{stages.Length}";
                throw new IndexOutOfRangeException(msg);
            }

            return id;
        }

        protected override void Process()
        {
            if(manager.HasFinished)
            {
                Game.LogTrivial($"Case: {data.ID} was finished");
                SetScriptFinished(true);
            }
        }

        protected override void End()
        {
            Game.LogTrivial($"Case: {data.ID}.End()");

            data.ModifyCaseProgress(p => p.Finished = true);
        }
    }
}
