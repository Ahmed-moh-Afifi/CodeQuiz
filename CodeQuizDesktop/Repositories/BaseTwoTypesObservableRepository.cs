using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public class BaseTwoTypesObservableRepository<T, G> : ITwoTypesObservableRepository<T, G>
    {
        private List<Action<T>> createSubscribers1 = [];
        private List<Action<T>> updateSubscribers1 = [];
        private List<Action<int>> deleteSubs1cribers1 = [];

        private List<Action<G>> createSubscribers2 = [];
        private List<Action<G>> updateSubscribers2 = [];
        private List<Action<int>> deleteSubs1cribers2 = [];

        public void NotifyCreate<C>(C createdItem)
        {
            if (typeof(C) == typeof(T))
            {
                foreach (var a in createSubscribers1)
                {
                    a.Invoke((T)(object)createdItem!);
                }
            }

            if (typeof(C) == typeof(G))
            {
                foreach (var a in createSubscribers2)
                {
                    a.Invoke((G)(object)createdItem!);
                }
            }
        }

        public void NotifyDelete<C>(int deletedItemId)
        {
            if (typeof(C) == typeof(T))
            {
                foreach (var a in deleteSubs1cribers1)
                {
                    a.Invoke(deletedItemId);
                }
            }

            if (typeof(C) == typeof(G))
            {
                foreach (var a in deleteSubs1cribers2)
                {
                    a.Invoke(deletedItemId);
                }
            }
        }

        public void NotifyUpdate<C>(C updatedItem)
        {
            if (typeof(C) == typeof(T))
            {
                foreach (var a in updateSubscribers1)
                {
                    a.Invoke((T)(object)updatedItem!);
                }
            }

            if (typeof(C) == typeof(G))
            {
                foreach (var a in updateSubscribers2)
                {
                    a.Invoke((G)(object)updatedItem!);
                }
            }
        }

        public void SubscribeCreate<C>(Action<C> action)
        {
            if (typeof(C) == typeof(T))
            {
                var actionT = (Action<T>)(object)action;
                if (!createSubscribers1.Contains(actionT))
                {
                    createSubscribers1.Add(actionT);
                }
            }

            if (typeof(C) == typeof(G))
            {
                var actionG = (Action<G>)(object)action;
                if (!createSubscribers2.Contains(actionG))
                {
                    createSubscribers2.Add(actionG);
                }
            }
        }

        public void SubscribeDetele<C>(Action<int> action)
        {
            if (typeof(C) == typeof(T))
            {
                if (!deleteSubs1cribers1.Contains(action))
                {
                    deleteSubs1cribers1.Add(action);
                }
            }

            if (typeof(C) == typeof(G))
            {
                if (!deleteSubs1cribers2.Contains(action))
                {
                    deleteSubs1cribers2.Add(action);
                }
            }
        }

        public void SubscribeUpdate<C>(Action<C> action)
        {
            if (typeof(C) == typeof(T))
            {
                var actionT = (Action<T>)(object)action;
                if (!updateSubscribers1.Contains(actionT))
                {
                    updateSubscribers1.Add(actionT);
                }
            }

            if (typeof(C) == typeof(G))
            {
                var actionG = (Action<G>)(object)action;
                if (!updateSubscribers2.Contains(actionG))
                {
                    updateSubscribers2.Add(actionG);
                }
            }
        }

        public void UnsubscribeCreate<C>(Action<C> action)
        {
            if (typeof(C) == typeof(T))
            {
                var actionT = (Action<T>)(object)action;
                createSubscribers1.Remove(actionT);
            }

            if (typeof(C) == typeof(G))
            {
                var actionG = (Action<G>)(object)action;
                createSubscribers2.Remove(actionG);
            }
        }

        public void UnsubscribeDetele<C>(Action<int> action)
        {
            if (typeof(C) == typeof(T))
            {
                deleteSubs1cribers1.Remove(action);
            }

            if (typeof(C) == typeof(G))
            {
                deleteSubs1cribers2.Remove(action);
            }
        }

        public void UnsubscribeUpdate<C>(Action<C> action)
        {
            if (typeof(C) == typeof(T))
            {
                var actionT = (Action<T>)(object)action;
                updateSubscribers1.Remove(actionT);
            }

            if (typeof(C) == typeof(G))
            {
                var actionG = (Action<G>)(object)action;
                updateSubscribers2.Remove(actionG);
            }
        }
    }
}
