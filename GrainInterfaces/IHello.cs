namespace GrainInterfaces;

public interface IHello : IGrainWithIntegerKey
{
    ValueTask<string> SayHello(string greeting);

    //Task SendMessage(string message);
}