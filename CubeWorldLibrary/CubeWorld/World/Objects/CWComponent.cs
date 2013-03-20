namespace CubeWorld.World.Objects
{
    public class CWComponent
    {
        protected CWObject cwobject;

        internal void AddedToObject(CWObject cwobject)
        {
            this.cwobject = cwobject;

            OnAddedToObject(cwobject);
        }

        internal void RemovedFromObject()
        {
            this.cwobject = null;

            OnRemovedFromObject();
        }

        protected virtual void OnRemovedFromObject()
        {
        }

        protected virtual void OnAddedToObject(CWObject cwobject)
        {
        }

        public virtual void Update(float deltaTime)
        {
        }
    }
}
