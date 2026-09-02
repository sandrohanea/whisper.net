// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

public struct WhisperFactoryOptions
{
    public static WhisperFactoryOptions Default => new();

    public WhisperFactoryOptions()
    {
        // Default values
        ModelFamily = WhisperModelFamily.Whisper;
        UseGpu = true;
        UseFlashAttention = false;
        UseDtwTimeStamps = false;
        HeadsPreset = WhisperAlignmentHeadsPreset.None;
        GpuDevice = 0;
        DtwMemSize = 1024 * 1024 * 128;
        DtwNTop = -1;
        DelayInitialization = false;
    }

    /// <summary>
    /// Gets or sets the native engine and model family used by the factory.
    /// </summary>
    /// <remarks>
    /// The default is <see cref="WhisperModelFamily.Whisper"/>.
    /// </remarks>
    public WhisperModelFamily ModelFamily { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use GPU for processing.
    /// </summary>
    /// <remarks>
    /// By default, it is true.
    /// </remarks>
    public bool UseGpu { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use FlashAttention.
    /// </summary>
    /// <remarks>
    /// By default, it is false.
    /// </remarks>
    public bool UseFlashAttention { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use Dynamic Time Warping (DTW) time stamps.
    /// </summary>
    /// <remarks>
    /// By default, it is false.
    /// </remarks>
    public bool UseDtwTimeStamps { get; set; }

    /// <summary>
    /// Gets or sets the alignment heads preset for DTW.
    /// </summary>
    /// <remarks>
    /// By default, it is <see cref="WhisperAlignmentHeadsPreset.None"/>.
    /// </remarks>
    public WhisperAlignmentHeadsPreset HeadsPreset { get; set; }

    /// <summary>
    /// Gets or sets the custom alignment heads for DTW.
    /// </summary>
    /// <remarks>
    /// By default, it is null. Required when using DTW with models which don't have a matching <see cref="WhisperAlignmentHeadsPreset"/>.
    /// </remarks>
    public WhisperAlignmentHead[]? CustomAlignmentHeads { get; set; }

    /// <summary>
    /// Gets or sets the GPU device to use for processing.
    /// </summary>
    /// <remarks>
    /// By default, it is 0.
    /// </remarks>
    public int GpuDevice { get; set; }

    /// <summary>
    /// Gets or sets the size of the DTW memory.
    /// </summary>
    /// <remarks>
    /// By default, it is 128 MB.
    /// </remarks>
    public uint DtwMemSize { get; set; }

    /// <summary>
    /// Gets or sets the N-top for DTW.
    /// </summary>
    /// <remarks>
    /// By default, it is -1.
    /// </remarks>
    public int DtwNTop { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to delay initialization of the Whisper context to the first call of <see cref="WhisperFactory.CreateBuilder"/>.
    /// </summary>
    /// <remarks>
    /// By default, it is false and the model is loaded right away.
    /// </remarks>
    public bool DelayInitialization { get; set; }

    internal readonly void Validate()
    {
        if (!Enum.IsDefined(typeof(WhisperModelFamily), ModelFamily))
        {
            throw new ArgumentOutOfRangeException(nameof(ModelFamily), ModelFamily, "Unknown model family.");
        }

        if (ModelFamily != WhisperModelFamily.Parakeet)
        {
            return;
        }

        if (UseFlashAttention)
        {
            throw new NotSupportedException("Flash attention is not supported by the Parakeet engine.");
        }

        if (UseDtwTimeStamps
            || HeadsPreset != WhisperAlignmentHeadsPreset.None
            || CustomAlignmentHeads is not null)
        {
            throw new NotSupportedException("DTW timestamps and alignment heads are not supported by the Parakeet engine.");
        }
    }
}
