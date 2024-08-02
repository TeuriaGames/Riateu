namespace Riateu.Audios;

public interface IVoice 
{
    int AudioTypeID { get; }
    const int SourceVoice = 0;
    const int SubmixVoice = 1;
}
