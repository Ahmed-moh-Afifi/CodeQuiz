using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Repositories
{
    public interface ITwoTypesObservableRepository<T, G>
    {
        void SubscribeCreate<C>(Action<C> action);
        void SubscribeUpdate<C>(Action<C> action);
        void SubscribeDetele<C>(Action<int> action);
        void UnsubscribeCreate<C>(Action<C> action);
        void UnsubscribeUpdate<C>(Action<C> action);
        void UnsubscribeDetele<C>(Action<int> action);
        void NotifyCreate<C>(C createdItem);
        void NotifyUpdate<C>(C updatedItem);
        void NotifyDelete<C>(int deletedItemId);
    }
}
