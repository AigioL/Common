/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System.Globalization;

namespace SAM.API;

public sealed partial class Client : IDisposable
{
    public bool IsConnectToSteam { get; set; }

    private bool _IsDisposed = false;

    private int _Pipe;

    private int _User;

    private readonly List<ICallback> _Callbacks = new();

    const string KEY_STEAM_APP_ID = "SteamAppId";

    public static bool WriteSteamAppIdTxt { get; set; }

    public bool Initialize(long appId, bool writeAppIdTxt = true)
    {
        if (string.IsNullOrEmpty(Steam.GetInstallPath()) == true)
        {
            throw new ClientInitializeException(ClientInitializeFailure.GetInstallPath, "failed to get Steam install path");
        }

        string? steam_appid_file_path = null;

        try
        {
            if (appId != 0)
            {
                var appIdString = appId.ToString(CultureInfo.InvariantCulture);
                Environment.SetEnvironmentVariable(KEY_STEAM_APP_ID, appIdString);

                if (Steam.Load() == false)
                {
                    if (writeAppIdTxt)
                    {
                        var processPath = Environment.ProcessPath;
                        ArgumentNullException.ThrowIfNull(processPath);
                        var processDirectory = Path.GetDirectoryName(processPath);
                        ArgumentNullException.ThrowIfNull(processDirectory);
                        steam_appid_file_path = Path.Combine(processDirectory, "steam_appid.txt");
                        if (Directory.Exists(steam_appid_file_path))
                        {
                            Directory.Delete(steam_appid_file_path, true);
                        }
                        File.WriteAllText(steam_appid_file_path, appIdString);

                        if (Steam.Load() == false)
                        {
                            throw new ClientInitializeException(ClientInitializeFailure.Load, "failed to load Steam");
                        }
                    }
                    else
                    {
                        throw new ClientInitializeException(ClientInitializeFailure.Load, "failed to load Steam");
                    }
                }
            }
            else
            {
                if (Steam.Load() == false)
                {
                    throw new ClientInitializeException(ClientInitializeFailure.Load, "failed to load Steam");
                }
            }

            _SteamClient = Steam.CreateInterface<Wrappers.SteamClient018>("SteamClient018")
                ?? throw new ClientInitializeException(ClientInitializeFailure.CreateSteamClient, "failed to create ISteamClient018");

            _Pipe = SteamClient.CreateSteamPipe();
            if (_Pipe == 0)
            {
                throw new ClientInitializeException(ClientInitializeFailure.CreateSteamPipe, "failed to create pipe");
            }

            _User = SteamClient.ConnectToGlobalUser(_Pipe);
            if (_User == 0)
            {
                throw new ClientInitializeException(ClientInitializeFailure.ConnectToGlobalUser, "failed to connect to global user");
            }

            _SteamUtils = SteamClient.GetSteamUtils007(_Pipe)
                ?? throw new ClientInitializeException(ClientInitializeFailure.CreateSteamClient, "failed to create ISteamClient018");
            if (appId > 0 && _SteamUtils.GetAppId() != unchecked((uint)appId))
            {
                throw new ClientInitializeException(ClientInitializeFailure.AppIdMismatch, $"appID mismatch, appId: {appId}");
            }

            SteamUser = SteamClient.GetSteamUser017(_User, _Pipe);
            SteamUserStats = SteamClient.GetSteamUserStats013(_User, _Pipe);
            SteamApps001 = SteamClient.GetSteamApps001(_User, _Pipe);
            SteamApps008 = SteamClient.GetSteamApps008(_User, _Pipe);
            SteamRemoteStorage = SteamClient.GetSteamRemoteStorage012(_User, _Pipe);
        }
        finally
        {
            if (steam_appid_file_path != null)
            {
                try
                {
                    File.Delete(steam_appid_file_path);
                }
                catch
                {

                }
            }
        }

        return true;
    }

    ~Client()
    {
        Dispose(false);
    }

    void Dispose(bool disposing)
    {
        if (_IsDisposed == true)
        {
            return;
        }

        _IsDisposed = true;

        if (SteamClient != null && _Pipe > 0)
        {
            if (_User > 0)
            {
                SteamClient.ReleaseUser(_Pipe, _User);
                _User = 0;
            }

            SteamClient.ReleaseSteamPipe(_Pipe);
            _Pipe = 0;
        }

        _IsDisposed = false;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public TCallback CreateAndRegisterCallback<TCallback>()
        where TCallback : ICallback, new()
    {
        var callback = new TCallback();
        _Callbacks.Add(callback);
        return callback;
    }

    private bool _RunningCallbacks;

    public void RunCallbacks(bool server)
    {
        if (_RunningCallbacks == true)
        {
            return;
        }

        _RunningCallbacks = true;

        while (Steam.GetCallback(_Pipe, out Types.CallbackMessage message, out int call) == true)
        {
            var callbackId = message.Id;
            foreach (ICallback callback in _Callbacks.Where(
                candidate => candidate.Id == callbackId &&
                             candidate.IsServer == server))
            {
                callback.Run(message.ParamPointer);
            }

            Steam.FreeLastCallback(_Pipe);
        }

        _RunningCallbacks = false;
    }
}

partial class Client // NativeWrappers
{
    Wrappers.SteamClient018? _SteamClient;
    Wrappers.SteamUser017? _SteamUser;
    Wrappers.SteamUserStats013? _SteamUserStats;
    Wrappers.SteamUtils007? _SteamUtils;
    Wrappers.SteamApps001? _SteamApps001;
    Wrappers.SteamApps008? _SteamApps008;
    Wrappers.SteamRemoteStorage012? _SteamRemoteStorage;
    Wrappers.SteamInventory002? _SteamInventory;

    public Wrappers.SteamClient018 SteamClient
    {
        get
        {
            ArgumentNullException.ThrowIfNull(_SteamClient);
            return _SteamClient;
        }
        set => _SteamClient = value;
    }

    public Wrappers.SteamUser017 SteamUser
    {
        get
        {
            ArgumentNullException.ThrowIfNull(_SteamUser);
            return _SteamUser;
        }
        set => _SteamUser = value;
    }

    public Wrappers.SteamUserStats013 SteamUserStats
    {
        get
        {
            ArgumentNullException.ThrowIfNull(_SteamUserStats);
            return _SteamUserStats;
        }
        set => _SteamUserStats = value;
    }

    public Wrappers.SteamUtils007 SteamUtils
    {
        get
        {
            ArgumentNullException.ThrowIfNull(_SteamUtils);
            return _SteamUtils;
        }
        set => _SteamUtils = value;
    }

    public Wrappers.SteamApps001 SteamApps001
    {
        get
        {
            ArgumentNullException.ThrowIfNull(_SteamApps001);
            return _SteamApps001;
        }
        set => _SteamApps001 = value;
    }

    public Wrappers.SteamApps008 SteamApps008
    {
        get
        {
            ArgumentNullException.ThrowIfNull(_SteamApps008);
            return _SteamApps008;
        }
        set => _SteamApps008 = value;
    }

    public Wrappers.SteamRemoteStorage012 SteamRemoteStorage
    {
        get
        {
            ArgumentNullException.ThrowIfNull(_SteamRemoteStorage);
            return _SteamRemoteStorage;
        }
        set => _SteamRemoteStorage = value;
    }

    public Wrappers.SteamInventory002 SteamInventory
    {
        get
        {
            ArgumentNullException.ThrowIfNull(_SteamInventory);
            return _SteamInventory;
        }
        set => _SteamInventory = value;
    }
}