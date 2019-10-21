﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Dialogues.Activities;
using Dialogues.Data;
using HPNS.Interactivity.Activities;
using HPNS.Interactivity.Core.Data;
using HPNS.Interactivity.Core.Task;
using HPNS.Interactivity.Support;
using HPNS.UI;

namespace Dialogues.Tasks
{
    public class LastResponseTask : TaskBase
    {
        private Response _response;
        private CameraConfiguration _configuration;
        private int _delay;
        
        private ITask _responseSequence;
        
        public IParameter<int> PedHandle;
        
        public LastResponseTask(Response response, CameraConfiguration configuration, int delay) : base(nameof(LastResponseTask))
        {
            _response = response;
            _configuration = configuration;
            _delay = delay;
        }

        protected override async Task ExecutePrepare()
        {
            var tasks = new List<ITask>();
            tasks.Add(new ParallelActivityTask(
                new SequenceTask(new List<ITask>
                {
                    new ParallelActivityTask(
                        new SequenceTask(new List<ITask>
                        {
                            new LambdaTask(() => UI.Dialogues.Print(_response.Participant.Name, _response.Content)),
                            new DialogueDidPrintWaitTask()
                        }),
                        new PlayFacialAnimActivity("mp_facial", "mic_chatter") {PedHandle = PedHandle}),
                    new WaitTask {Duration = new Parameter<int>(1500)},
                    new LambdaTask(() => UI.Dialogues.Stop()),
                    new WaitTask {Duration = new Parameter<int>(_delay)}
                }),
                new PedCameraActivity(_configuration) {PedHandle = PedHandle}));
            tasks.Add(new LambdaTask(NotifyTaskDidEnd));

            _responseSequence = new SequenceTask(tasks);

            await _responseSequence.Prepare();
        }

        protected override void ExecuteStart()
        {
            _responseSequence.Start();
        }

        protected override void ExecuteAbort()
        {
            _responseSequence.Abort();
        }

        protected override void ExecuteReset()
        {
            _responseSequence.Reset();
        }
    }
}