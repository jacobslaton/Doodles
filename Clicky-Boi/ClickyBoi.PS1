# https://stackoverflow.com/questions/43755134/c-sharp-moving-clicking-mouse-winforms
# https://devblogs.microsoft.com/scripting/use-powershell-to-interact-with-the-windows-api-part-1/#using-add-type-to-call-the-copyitem-function
# https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys?view=net-5.0

Add-Type -AssemblyName System.Windows.forms
$signature = @"
[DllImport("user32.dll")]
public static extern void mouse_event(
	uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo
);

static public void Click(uint X, uint Y)
{
	// MOUSEEVENTF_LEFTDOWN 0x0002
	// MOUSEEVENTF_LEFTUP 0x0004
	mouse_event(0x0002 | 0x0004, X, Y, 0, 0);
}
"@
$ClickyBoi = Add-Type `
	-MemberDefinition $signature `
	-Name "ClickyBoi" `
	-Namespace "User32" `
	-PassThru
Function Click {
	Param (
		[Parameter(Mandatory=$true)]$posX,
		[Parameter(Mandatory=$true)]$posY
	)

	[Windows.Forms.Cursor]::Position = "$($posX),$($posY)"
	$ClickyBoi::Click( `
		[Windows.Forms.Cursor]::Position.X,
		[Windows.Forms.Cursor]::Position.Y
	)
}
Function TypeKeys {
	Param (
		[Parameter(Mandatory=$true)]$keys
	)

	[System.Windows.Forms.SendKeys]::SendWait($keys)
}

# Click 0 0
# TypeKeys "Hello World!"
# Start-Sleep -Milliseconds 500
