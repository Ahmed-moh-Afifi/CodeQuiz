using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public interface IObservableRepository<T>
    {
        void SubscribeCreate(Action<T> action);
        void SubscribeUpdate(Action<T> action);
        void SubscribeDetele(Action<int> action);
        void UnsubscribeCreate(Action<T> action);
        void UnsubscribeUpdate(Action<T> action);
        void UnsubscribeDetele(Action<int> action);
        void NotifyCreate(T createdItem);
        void NotifyUpdate(T updatedItem);
        void NotifyDelete(int deletedItemId);
    }
}
