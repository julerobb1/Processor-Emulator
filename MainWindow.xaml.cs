<Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window><Window x:Class="ProcessorEmulator.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Processor Emulator" Height="450" Width="800">
    <DockPanel>
        <Menu DockPanel.Dock="Top">
            <MenuItem Header="_File">
                <MenuItem Header="_Open" Click="OpenMenuItem_Click"/>
                <!-- General firmware extract & analysis -->
                <MenuItem Header="Analyze _Firmware..." Click="AnalyzeFirmware_Click"/>
                <MenuItem Header="Scan _DVR Data..." Click="ScanDvrData_Click" ToolTip="Scan attached DVR datasets for firmware and logs"/>
                <MenuItem Header="List DVR _Firmware" Click="ListDvrFirmware_Click" ToolTip="List all firmware files in DVR datasets"/>
                <MenuItem Header="Analyze _All DVR Data" Click="AnalyzeAllDvrData_Click" ToolTip="Perform full DVR dataset analysis (firmware, XFS, configs)"/>
                <Separator/>
                <MenuItem Header="_Exit" />
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="Use Unicorn Engine"
                          ToolTip="Toggle in-process Unicorn stub engine for same-architecture execution-based translation"
                          IsCheckable="True"
                          Checked="UseUnicorn_Checked"
                          Unchecked="UseUnicorn_Unchecked"/>
            </MenuItem>
            <MenuItem Header="_Help" />
        </Menu>
        <StatusBar DockPanel.Dock="Bottom">
            <StatusBarItem Content="Ready" />
        </StatusBar>
        <Grid>
            <TabControl>
                <TabItem Header="Emulation">
                    <StackPanel Margin="10">
                        <TextBlock FontWeight="Bold" Text="Firmware Boot" />
                        <DockPanel Margin="0,5">
                            <TextBox x:Name="FirmwarePathTextBox" Width="400"/>
                            <Button x:Name="BrowseFirmwareButton" Content="Browse..." Width="75" Margin="5,0,0,0" Click="BrowseFirmwareButton_Click"/>
                        </DockPanel>
                        <StackPanel Orientation="Horizontal" Margin="0,5" HorizontalAlignment="Left">
                            <Button x:Name="BootFirmwareButton" Content="Boot Firmware" Width="100" Margin="0,0,5,0" Click="BootFirmwareButton_Click"/>
                            <Button x:Name="StartEmulationButton" Content="Start Emulator" Width="100" Click="StartEmulationButton_Click"/>
                        </StackPanel>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Analysis">
                    <StackPanel Margin="10">
                        <Button Content="Analyze Firmware" Width="150" Margin="0,5" Click="AnalyzeFirmware_Click"/>
                        <Button Content="Extract Firmware" Width="150" Margin="0,5" Click="ExtractFirmware_Click"/>
                        <Button Content="Detect File Type" Width="150" Margin="0,5" Click="DetectFileType_Click"/>
                        <Button Content="Summarize DVR Data" Width="150" Margin="0,5" Click="SummarizeDvrData_Click"/>
                    </StackPanel>
                </TabItem>
                <TabItem Header="Filesystems">
                    <StackPanel Margin="10">
                        <Button Content="Mount FAT" Width="150" Margin="0,5" Click="MountFat_Click"/>
                        <Button Content="Mount ISO" Width="150" Margin="0,5" Click="MountIso_Click"/>
                        <Button Content="Mount EXT" Width="150" Margin="0,5" Click="MountExt_Click"/>
                        <Button Content="Mount SquashFS" Width="150" Margin="0,5" Click="MountSquashFs_Click"/>
                    </StackPanel>
                </TabItem>
            </TabControl>
        </Grid>
    </DockPanel>
</Window>