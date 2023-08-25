// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <wrl/module.h>
#include <wrl/implements.h>
#include <wrl/client.h>
#include <shobjidl_core.h>
#include <wil\resource.h>
#include <string>
#include <vector>
#include <sstream>

using namespace Microsoft::WRL;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

class TestExplorerCommandBase : public RuntimeClass<RuntimeClassFlags<ClassicCom>, IExplorerCommand, IObjectWithSite>
{
public:
    virtual const wchar_t* Title() = 0;
    virtual const EXPCMDFLAGS Flags() { return ECF_DEFAULT; }
    virtual const EXPCMDSTATE State(_In_opt_ IShellItemArray* selection) { return ECS_ENABLED; }

    // IExplorerCommand
    IFACEMETHODIMP GetTitle(_In_opt_ IShellItemArray* items, _Outptr_result_nullonfailure_ PWSTR* name)
    {
        *name = nullptr;
        auto title = wil::make_cotaskmem_string_nothrow(Title());
        RETURN_IF_NULL_ALLOC(title);
        *name = title.release();
        return S_OK;
    }
    IFACEMETHODIMP GetIcon(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* icon) { *icon = nullptr; return E_NOTIMPL; }
    IFACEMETHODIMP GetToolTip(_In_opt_ IShellItemArray*, _Outptr_result_nullonfailure_ PWSTR* infoTip) { *infoTip = nullptr; return E_NOTIMPL; }
    IFACEMETHODIMP GetCanonicalName(_Out_ GUID* guidCommandName) { *guidCommandName = GUID_NULL;  return S_OK; }
// This function determines the state of a command based on certain conditions.
// It takes a selection of items, a flag indicating whether slow processing is acceptable,
// and returns the state of the command in the 'cmdState' parameter.
IFACEMETHODIMP GetState(_In_opt_ IShellItemArray* selection, _In_ BOOL okToBeSlow, _Out_ EXPCMDSTATE* cmdState)
{
    // Check if there is a selection and slow processing is allowed.
    if (selection && okToBeSlow)
    {
        // If both conditions are met, enable the command.
        // This means the command will be visible and usable.
        *cmdState = ECS_ENABLED;
        return S_OK;  // Return success status.
    }

    // If the conditions are not met, hide the command.
    // This means the command won't be shown under "Show More Options to the user".
    *cmdState = ECS_HIDDEN;
    return S_OK;  // Return success status.
}
    IFACEMETHODIMP Invoke(_In_opt_ IShellItemArray* selection, _In_opt_ IBindCtx*) noexcept try
    {
        HWND parent = nullptr;
        if (m_site)
        {
            ComPtr<IOleWindow> oleWindow;
            RETURN_IF_FAILED(m_site.As(&oleWindow));
            RETURN_IF_FAILED(oleWindow->GetWindow(&parent));
        }

        std::wostringstream title;
        title << Title();

        if (selection)
        {
            DWORD count;
            RETURN_IF_FAILED(selection->GetCount(&count));
            title << L" (" << count << L" selected items)";
        }
        else
        {
            title << L"(no selected items)";
        }

        MessageBox(parent, title.str().c_str(), L"TestCommand", MB_OK);
        return S_OK;
    }
    CATCH_RETURN();

    IFACEMETHODIMP GetFlags(_Out_ EXPCMDFLAGS* flags) { *flags = Flags(); return S_OK; }
    IFACEMETHODIMP EnumSubCommands(_COM_Outptr_ IEnumExplorerCommand** enumCommands) { *enumCommands = nullptr; return E_NOTIMPL; }

    // IObjectWithSite
    IFACEMETHODIMP SetSite(_In_ IUnknown* site) noexcept { m_site = site; return S_OK; }
    IFACEMETHODIMP GetSite(_In_ REFIID riid, _COM_Outptr_ void** site) noexcept { return m_site.CopyTo(riid, site); }

protected:
    ComPtr<IUnknown> m_site;
};

class __declspec(uuid("3282E233-C5D3-4533-9B25-44B8AAAFACFA")) TestExplorerCommandHandler final : public TestExplorerCommandBase
{
public:
    const wchar_t* Title() override { return L"PhotoStore Command1"; }
    const EXPCMDSTATE State(_In_opt_ IShellItemArray* selection) override { return ECS_DISABLED; }
};

class __declspec(uuid("817CF159-A4B5-41C8-8E8D-0E23A6605395")) TestExplorerCommand2Handler final : public TestExplorerCommandBase
{
public:
    const wchar_t* Title() override { return L"PhotoStore ExplorerCommand2"; }
};

class SubExplorerCommandHandler final : public TestExplorerCommandBase
{
public:
    const wchar_t* Title() override { return L"SubCommand"; }
};

class CheckedSubExplorerCommandHandler final : public TestExplorerCommandBase
{
public:
    const wchar_t* Title() override { return L"CheckedSubCommand"; }
    const EXPCMDSTATE State(_In_opt_ IShellItemArray* selection) override { return ECS_CHECKBOX | ECS_CHECKED; }
};

class RadioCheckedSubExplorerCommandHandler final : public TestExplorerCommandBase
{
public:
    const wchar_t* Title() override { return L"RadioCheckedSubCommand"; }
    const EXPCMDSTATE State(_In_opt_ IShellItemArray* selection) override { return ECS_CHECKBOX | ECS_RADIOCHECK; }
};

class HiddenSubExplorerCommandHandler final : public TestExplorerCommandBase
{
public:
    const wchar_t* Title() override { return L"HiddenSubCommand"; }
    const EXPCMDSTATE State(_In_opt_ IShellItemArray* selection) override { return ECS_HIDDEN; }
};

class EnumCommands : public RuntimeClass<RuntimeClassFlags<ClassicCom>, IEnumExplorerCommand>
{
public:
    EnumCommands()
    {
        m_commands.push_back(Make<SubExplorerCommandHandler>());
        m_commands.push_back(Make<CheckedSubExplorerCommandHandler>());
        m_commands.push_back(Make<RadioCheckedSubExplorerCommandHandler>());
        m_commands.push_back(Make<HiddenSubExplorerCommandHandler>());
        m_current = m_commands.cbegin();
    }

    // IEnumExplorerCommand
    IFACEMETHODIMP Next(ULONG celt, __out_ecount_part(celt, *pceltFetched) IExplorerCommand** apUICommand, __out_opt ULONG* pceltFetched)
    {
        ULONG fetched{ 0 };
        wil::assign_to_opt_param(pceltFetched, 0ul);

        for (ULONG i = 0; (i < celt) && (m_current != m_commands.cend()); i++)
        {
            m_current->CopyTo(&apUICommand[0]);
            m_current++;
            fetched++;
        }

        wil::assign_to_opt_param(pceltFetched, fetched);
        return (fetched == celt) ? S_OK : S_FALSE;
    }

    IFACEMETHODIMP Skip(ULONG /*celt*/) { return E_NOTIMPL; }
    IFACEMETHODIMP Reset()
    {
        m_current = m_commands.cbegin();
        return S_OK;
    }
    IFACEMETHODIMP Clone(__deref_out IEnumExplorerCommand** ppenum) { *ppenum = nullptr; return E_NOTIMPL; }

private:
    std::vector<ComPtr<IExplorerCommand>> m_commands;
    std::vector<ComPtr<IExplorerCommand>>::const_iterator m_current;
};

class __declspec(uuid("1476525B-BBC2-4D04-B175-7E7D72F3DFF8")) TestExplorerCommand3Handler final : public TestExplorerCommandBase
{
public:
    const wchar_t* Title() override { return L"PhotoStore CommandWithSubCommands"; }
    const EXPCMDFLAGS Flags() override { return ECF_HASSUBCOMMANDS; }

    IFACEMETHODIMP EnumSubCommands(_COM_Outptr_ IEnumExplorerCommand** enumCommands)
    {
        *enumCommands = nullptr;
        auto e = Make<EnumCommands>();
        return e->QueryInterface(IID_PPV_ARGS(enumCommands));
    }
};

class __declspec(uuid("30DEEDF6-63EA-4042-A7D8-0A9E1B17BB99")) TestExplorerCommand4Handler final : public TestExplorerCommandBase
{
public:
    const wchar_t* Title() override { return L"PhotoStore Command4"; }
};

class __declspec(uuid("50419A05-F966-47BA-B22B-299A95492348")) TestExplorerHiddenCommandHandler final : public TestExplorerCommandBase
{
public:
    const wchar_t* Title() override { return L"PhotoStore HiddenCommand"; }
    const EXPCMDSTATE State(_In_opt_ IShellItemArray* selection) override { return ECS_HIDDEN; }
};

CoCreatableClass(TestExplorerCommandHandler)
CoCreatableClass(TestExplorerCommand2Handler)
CoCreatableClass(TestExplorerCommand3Handler)
CoCreatableClass(TestExplorerCommand4Handler)
CoCreatableClass(TestExplorerHiddenCommandHandler)

CoCreatableClassWrlCreatorMapInclude(TestExplorerCommandHandler)
CoCreatableClassWrlCreatorMapInclude(TestExplorerCommand2Handler)
CoCreatableClassWrlCreatorMapInclude(TestExplorerCommand3Handler)
CoCreatableClassWrlCreatorMapInclude(TestExplorerCommand4Handler)
CoCreatableClassWrlCreatorMapInclude(TestExplorerHiddenCommandHandler)


STDAPI DllGetActivationFactory(_In_ HSTRING activatableClassId, _COM_Outptr_ IActivationFactory** factory)
{
    return Module<ModuleType::InProc>::GetModule().GetActivationFactory(activatableClassId, factory);
}

STDAPI DllCanUnloadNow()
{
    return Module<InProc>::GetModule().GetObjectCount() == 0 ? S_OK : S_FALSE;
}

STDAPI DllGetClassObject(_In_ REFCLSID rclsid, _In_ REFIID riid, _COM_Outptr_ void** instance)
{
    return Module<InProc>::GetModule().GetClassObject(rclsid, riid, instance);
}
