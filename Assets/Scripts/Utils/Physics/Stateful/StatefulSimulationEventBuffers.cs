using Unity.Collections;


namespace Unity.Physics.Stateful
{
    public struct StatefulSimulationEventBuffers<T> where T : unmanaged, IStatefulSimulationEvent<T>
    {
        public NativeList<T> Previous;
        public NativeList<T> Current;
        
        
        /// <summary>
    /// Given two sorted event buffers, this function returns a single combined list with
    /// all the appropriate <see cref="StatefulEventState"/> set on each event.
    /// </summary>
    /// <param name="previousEvents">The events buffer from the previous frame. This list should have already be sorted from the previous frame.</param>
    /// <param name="currentEvents">The events buffer from the current frame. This list should be sorted before calling this function.</param>
    /// <param name="statefulEvents">A single combined list of stateful events based on the previous and current frames.</param>
    /// <param name="sortCurrent">Specifies whether the currentEvents list needs to be sorted first.</param>
    public static void GetStatefulEvents(NativeList<T> previousEvents, NativeList<T> currentEvents,
        NativeList<T> statefulEvents, bool sortCurrent = true)
    {
        if (sortCurrent) currentEvents.Sort();

        statefulEvents.Clear();

        int c = 0;
        int p = 0;
        while (c < currentEvents.Length && p < previousEvents.Length)
        {
            int r = previousEvents[p].CompareTo(currentEvents[c]);
            if (r == 0)
            {
                var currentEvent = currentEvents[c];
                currentEvent.State = StatefulEventState.Stay;
                statefulEvents.Add(currentEvent);
                c++;
                p++;
            }
            else if (r < 0)
            {
                var previousEvent = previousEvents[p];
                previousEvent.State = StatefulEventState.Exit;
                statefulEvents.Add(previousEvent);
                p++;
            }
            else //(r > 0)
            {
                var currentEvent = currentEvents[c];
                currentEvent.State = StatefulEventState.Enter;
                statefulEvents.Add(currentEvent);
                c++;
            }
        }

        if (c == currentEvents.Length)
        {
            while (p < previousEvents.Length)
            {
                var previousEvent = previousEvents[p];
                previousEvent.State = StatefulEventState.Exit;
                statefulEvents.Add(previousEvent);
                p++;
            }
        }
        else if (p == previousEvents.Length)
        {
            while (c < currentEvents.Length)
            {
                var currentEvent = currentEvents[c];
                currentEvent.State = StatefulEventState.Enter;
                statefulEvents.Add(currentEvent);
                c++;
            }
        }
    }
    }
    
    
    
    
    
}
