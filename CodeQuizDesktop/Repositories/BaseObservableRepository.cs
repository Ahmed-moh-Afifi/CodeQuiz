using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public class BaseObservableRepository<T> : IObservableRepository<T>
    {
        private List<Action<T>> createSubscribers = [];
        private List<Action<T>> updateSubscribers = [];
        private List<Action<int>> deleteSubscribers = [];

        public void NotifyCreate(T createdItem)
        {
            foreach (var a in createSubscribers)
            {
                a.Invoke(createdItem);
            }
        }

        public void NotifyDelete(int deletedItemId)
        {
            foreach (var a in deleteSubscribers)
            {
                a.Invoke(deletedItemId);
            }
        }

        public void NotifyUpdate(T updatedItem)
        {
            foreach (var a in updateSubscribers)
            {
                a.Invoke(updatedItem);
            }
        }

        public void SubscribeCreate(Action<T> action)
        {
            createSubscribers.Add(action);
        }

        public void SubscribeDetele(Action<int> action)
        {
            deleteSubscribers.Add(action);
        }

        public void SubscribeUpdate(Action<T> action)
        {
            updateSubscribers.Add(action);
        }

        public void UnsubscribeCreate(Action<T> action)
        {
            createSubscribers.Remove(action);
        }

        public void UnsubscribeDetele(Action<int> action)
        {
            deleteSubscribers.Remove(action);
        }

        public void UnsubscribeUpdate(Action<T> action)
        {
            updateSubscribers.Remove(action);
        }
    }
}
