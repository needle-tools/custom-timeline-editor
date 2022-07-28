![colorbar](https://user-images.githubusercontent.com/5083203/180309860-542e6882-163c-4e11-9555-2c669ad72472.png)


# Custom Timeline Keyframes and Data Painter Tools
*Custom keyframes for Unity's timeline allow for easy interpolation of any custom data type including visual tools and a modular tooling system to creatively manipulate your data for use as input of generative animation and more*

### Features
- Custom keyframes (keyframe and interpolate any collection of types)
- Undo / Redo
- C# to compute shader binding

See [videos below](#videos)

### State of the project
This tool was originally developed to ease animating with code and give me some flexible and visual tools to control generative animation. It should be in a working state but still rough around the edges of e.g. UX, is not tested in production of any real project. Yet I think it can be useful and I hope to return to it one day to improve all the things that need attention (e.g. better documentation, improving the UI and UX for the modular tools, fixing of bugs, improving performance, finish implementation of C# → compute binding).  

That being said: please [open issues](https://github.com/needle-tools/custom-timeline-editor/issues/new) if you have problems or contribute by [opening a PR](https://github.com/needle-tools/custom-timeline-editor/pulls) 🙏

### Samples
A minimal sample is in ``projects/Timeline-2020_3/Assets``. More examples for getting started can be found in [the playground repository](https://github.com/needle-tools/custom-timeline-playground). Currently samples are messy from different versions and experimentation 🧪 and need to be updated. [I'll try to cleanup and provide better samples soon](https://github.com/needle-tools/custom-timeline-editor/issues/37)  


### Dependencies
- [needle timeline package fork](https://github.com/needle-tools/com.unity.timeline)
- [needle custom undo package](https://github.com/needle-tools/Unity-Custom-Undo)

### Getting started
![image](https://user-images.githubusercontent.com/5083203/180650806-a2d35a3f-3c0f-4e68-b542-0e7fa36179a9.png)
1) Create a timeline, add a ``Code Control`` track to it. Right click in the control track to create a ``Control Track Asset`` (same as AnimationClip but for custom data, it can be re-used multiple times in a timeline or other timelines)
2) Create script that implements the ``IAnimated`` interface or derives from the ``Animated`` class:
```csharp
public class SimpleScript : Animated
{
    // Here is a custom type that you can control via timeline
    public class Point
    {   
        // a field named position will automatically be detected by the spray tool to be painted in 3d space
        public Vector3 Position;
        // you may name fields as you like and add as many as you want. They will show up in the tool to be painted and individually manipulated
        public float Radius = .1f;
    }
    
    // annotade any field that you want to animate via timeline with the Animate attribute:
    [Animate]
    public List<Point> MyList;

    // this is just for the sake of visualizing the painted points:
    private void OnDrawGizmos()
    {
        if (MyList == null) return;
        foreach (var pt in MyList)
        {
            Gizmos.DrawSphere(pt.Position, pt.Radius);
        }
    }
}
```
3) Add the script to your control track
4) Open the ``Tools/Timeline Tools`` window
5) Open the curve views of the Control Track and click the record button for the field in your script, then start painting data.


### Videos:

https://user-images.githubusercontent.com/5083203/180649590-dbba6339-95a7-4f47-a475-72e630d6c3f5.mp4

#### Modular tooling system [[tweet](https://twitter.com/marcel_wiessler/status/1461283048113291268)]:
https://user-images.githubusercontent.com/5083203/180649365-55a6b8c5-c20d-41ed-90da-bacc8730d91a.mp4

https://user-images.githubusercontent.com/5083203/180648913-fe8b71af-d767-461b-962b-c5de3314e13e.mp4


#### Any data type: control, paint and interpolate:
https://user-images.githubusercontent.com/5083203/180648981-44637e9c-44e5-4a92-a458-bcc6579c8fee.mp4

https://user-images.githubusercontent.com/5083203/180649133-12cecbfd-55eb-4b25-833b-1380d2de6ec1.mp4

#### Onion skin [[tweet](https://twitter.com/marcel_wiessler/status/1449838707054350342)]:
https://user-images.githubusercontent.com/5083203/180649173-bba2020c-715e-44ae-9572-d5c258cc78ed.mp4

#### Edit mode support:
https://user-images.githubusercontent.com/5083203/180648938-cdcbb2fb-a6d8-437f-a635-0fc3027bde23.mp4

#### Runtime support [[tweet](https://github.com/needle-tools/needle-tiny-playground/issues/305)]:
https://user-images.githubusercontent.com/5083203/180648819-957af815-a315-4d03-babf-65da785ec772.mp4

#### Simulation mode [[tweet](https://twitter.com/marcel_wiessler/status/1448775383239872512)]:
https://user-images.githubusercontent.com/5083203/180649073-1b795271-b152-454a-8000-1ab378f657c0.mp4

#### Easy to integrate (e.g. controlling [keijiro's swarm](https://github.com/keijiro/Swarm))
https://user-images.githubusercontent.com/5083203/180649232-48586913-b798-48ba-bc88-f593b08fbdad.mp4

#### Works with compute + can automatically bind and dispatch C# members to ComputeShaders ([tweet](https://twitter.com/marcel_wiessler/status/1552793904742793216))

https://user-images.githubusercontent.com/5083203/181651946-c203beb8-28bf-4013-8f59-32dad0f7d8dc.mp4



---
## Contact ✒️
<b>[🌵 needle — tools for unity](https://needle.tools)</b> • 
[@NeedleTools](https://twitter.com/NeedleTools) • 
[@marcel_wiessler](https://twitter.com/marcel_wiessler) • 
[@hybridherbst](https://twitter.com/hybridherbst) • 
[Needle Discord](http://discord.needle.tools)
