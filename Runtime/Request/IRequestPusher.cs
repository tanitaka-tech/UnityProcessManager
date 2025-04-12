namespace TanitakaTech.UnityProcessManager
{
    public interface IRequestPusher<TRequest>
    {
        void PushRequest(TRequest requestValue);
    }
}