using System;

namespace Infrastructure
{
    public interface IState: IExsitableState
    {
        void Enter();
    }

    public interface IExsitableState
    {
        void Exit();
    }
    
    public interface IPayloadedState<TPayload>: IExsitableState
    {
        void Enter(TPayload payload);
    }
}