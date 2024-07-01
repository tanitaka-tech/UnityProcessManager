namespace TanitakaTech.UnityProcessManager
{
    public interface IRequestPusher<TRequest>
    {
        public void PushRequest(TRequest requestValue);
    }
}