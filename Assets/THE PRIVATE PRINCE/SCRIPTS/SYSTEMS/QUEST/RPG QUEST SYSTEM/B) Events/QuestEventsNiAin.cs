using System;

namespace Ain
{
    public class QuestEvents
    {
        // ------------------------- EVENTS -------------------------

        public event Action<string> onStartQuest;
        public event Action<string> onAdvanceQuest;
        public event Action<string> onFinishQuest;
        public event Action<Quest> onQuestStateChange;
        
        // ------------------------ TRIGGERS -------------------------

        // Method to broadcast the Start of a Quest
        public void StartQuest(string id)
        {
            onStartQuest?.Invoke(id);
        }

        // Method to broadcast the Advancement of a Quest
        public void AdvanceQuest(string id)
        {
            onAdvanceQuest?.Invoke(id);
        }

        // Method to broadcast the Finished of a Quest
        public void FinishQuest(string id)
        {
            onFinishQuest?.Invoke(id);
        }

        // --------------------- CUSTOM TRIGGERS ------------------------

        // Method to broadcast the Update for a Quest Status
        public void QuestStateChange(Quest quest)
        {
            onQuestStateChange?.Invoke(quest);
        }
    }
}