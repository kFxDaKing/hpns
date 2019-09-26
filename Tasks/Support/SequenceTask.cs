﻿using System;
using System.Collections.Generic;
using HPNS.Tasks.Core;

namespace HPNS.Tasks.Support
{
    public class SequenceTask : ITask
    {
        private Queue<ITask> _tasks;
        private ITask _currentTask;
        
        public TaskState CurrentState { get; private set; } = TaskState.Waiting;
        
        public event EventHandler TaskDidEnd;

        public SequenceTask(IEnumerable<ITask> tasks)
        {
            _tasks = new Queue<ITask>(tasks);
        }
        
        public void Start()
        {
            if (CurrentState != TaskState.Waiting)
                throw new Exception("Cannot start task that is not in Waiting state!");

            if (_tasks.Count == 0)
            {
                CurrentState = TaskState.Ended;
                TaskDidEnd?.Invoke(this, EventArgs.Empty);
                
                return;
            }
            
            StartNextTask();
            
            CurrentState = TaskState.Running;
        }

        public void Abort()
        {
            if (CurrentState != TaskState.Running)
                throw new Exception("Cannot abort task that is not in Running state!");
            
            _currentTask.Abort();
            _currentTask = null;

            CurrentState = TaskState.Aborted;
        }

        private void StartNextTask()
        {
            if (_tasks.Count == 0)
            {
                CurrentState = TaskState.Ended;
                TaskDidEnd?.Invoke(this, EventArgs.Empty);

                return;
            }

            _currentTask = _tasks.Dequeue();
            _currentTask.TaskDidEnd += CurrentTaskOnTaskDidEnd;
            _currentTask.Start();
        }

        private void CurrentTaskOnTaskDidEnd(object sender, EventArgs e)
        {
            _currentTask.TaskDidEnd -= CurrentTaskOnTaskDidEnd;
            _currentTask = null;
            
            StartNextTask();
        }
    }
}