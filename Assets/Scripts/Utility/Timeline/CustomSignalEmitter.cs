using UnityEngine;
using UnityEngine.Timeline;

namespace Utility.Timeline
{
    public class SignalEmitterSfx : ParameterizedEmitter<Object> { }
    
    public class SignalEmitterBgm : ParameterizedEmitter<Object> { }
    
    public class SignalEmitterSfxAsBgm : ParameterizedEmitter<Object> { }
    
    public class SignalEmitterStopSfx : ParameterizedEmitter<Object> { }

    public class ParameterizedEmitter<T> : SignalEmitter
    {
        public T parameter;
    }
}
