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
    public class ResponseTask : TaskBase
    {
        private Response _response;
        private CameraConfiguration _configuration;
        
        private ITask _responseSequence;
        
        public IParameter<int> FromPedHandle;
        public IParameter<int> ToPedHandle;
        
        public ResponseTask(Response response, CameraConfiguration configuration)
        {
            _response = response;
            _configuration = configuration;
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
                            new LambdaTask(() => UI.Dialogues.Print(_response.From.Name, _response.Content)),
                            new DialogueDidPrintWaitTask()
                        }),
                        new PlayFacialAnimActivity("mp_facial", "mic_chatter") {PedHandle = FromPedHandle}),
                    new WaitTask {Duration = new Parameter<int>(1500)}
                }),
                new PedCameraActivity(_configuration) {PedHandle = FromPedHandle},
                new PedLookAtEntityActivity {PedHandle = FromPedHandle, EntityHandle = ToPedHandle},
                new PedLookAtEntityActivity {PedHandle = ToPedHandle, EntityHandle = FromPedHandle}));
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