// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.Native;

internal static class NativeMethods
{
    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_init_from_file(string path);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, EntryPoint = "whisper_init_from_buffer", CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_init_from_buffer_x64(IntPtr buffer, long buffer_size);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, EntryPoint = "whisper_init_from_buffer", CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_init_from_buffer_x32(IntPtr buffer, int buffer_size);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void whisper_free(IntPtr context);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern WhisperFullParams whisper_full_default_params(WhisperSamplingStrategy strategy);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full(IntPtr context, WhisperFullParams parameters, IntPtr samples, int nSamples);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_parallel(IntPtr context, IntPtr state, WhisperFullParams parameters, IntPtr samples, int nSamples, int nThreads);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_n_segments(IntPtr state);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern long whisper_full_get_segment_t0(IntPtr state, int index);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern long whisper_full_get_segment_t1(IntPtr state, int index);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_full_get_segment_text(IntPtr state, int index);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_tokenize(IntPtr context, IntPtr text, IntPtr tokens, int nMaxTokens);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_lang_max_id();

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_lang_auto_detect(IntPtr context, IntPtr state, int offset_ms, int n_threads, IntPtr lang_probs);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_pcm_to_mel(IntPtr context, IntPtr state, IntPtr samples, int nSamples, int nThreads);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_pcm_to_mel_phase_vocoder(IntPtr context, IntPtr state, IntPtr samples, int nSamples, int nThreads);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_lang_str(int lang_id);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_lang_id(IntPtr context);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_init_state(IntPtr context);

    [DllImport("whisper", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void whisper_free_state(IntPtr state);
}