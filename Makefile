NAME :=          snakey
PROJNAME :=      snake-script
TARGET_FRMWRK := netcoreapp3.1

SRCFLDRS :=      src src/Parser src/Data src/VirtualMachine
SRCFILES :=      $(foreach folder,$(SRCFLDRS),$(wildcard $(folder)/*.cs))

ifdef LINUX
    RUNTIME := linux-x64
    OBJNAME := $(NAME)
    BINNAME := $(PROJNAME)
else
    ifdef LINUX_ARM
        RUNTIME := linux-arm
        OBJNAME := $(NAME)
        BINNAME := $(PROJNAME)
    else
        ifdef WIN32
            RUNTIME := win-x86
            OBJNAME := $(NAME).exe
            BINNAME := $(PROJNAME).exe
        else
            ifdef WIN64
                RUNTIME := win-x64
                OBJNAME := $(NAME).exe
                BINNAME := $(PROJNAME).exe
            endif
        endif
    endif
endif

LINUX_RUNTIMES := linux-x64 linux-arm
WIN_RUNTIMES := win-x86 win-x64
.PHONY : all
all : $(SRCFILES)
	dotnet publish $(PROJNAME).csproj -f $(TARGET_FRMWRK) -p:PublishSingleFile=true -r linux-x64
	cp bin/Debug/$(TARGET_FRMWRK)/linux-x64/publish/$(PROJNAME) ./snakey-linux-x64
	dotnet publish $(PROJNAME).csproj -f $(TARGET_FRMWRK) -p:PublishSingleFile=true -r linux-arm
	cp bin/Debug/$(TARGET_FRMWRK)/linux-arm/publish/$(PROJNAME) ./snakey-linux-arm
	dotnet publish $(PROJNAME).csproj -f $(TARGET_FRMWRK) -p:PublishSingleFile=true -r win-x86
	cp bin/Debug/$(TARGET_FRMWRK)/win-x86/publish/$(PROJNAME).exe ./snakey-win-x86.exe
	dotnet publish $(PROJNAME).csproj -f $(TARGET_FRMWRK) -p:PublishSingleFile=true -r win-x64
	cp bin/Debug/$(TARGET_FRMWRK)/win-x86/publish/$(PROJNAME).exe ./snakey-win-x64.exe

$(OBJNAME) : $(SRCFILES)
	dotnet publish $(PROJNAME).csproj -f $(TARGET_FRMWRK) -p:PublishSingleFile=true -r $(RUNTIME)
	cp bin/Debug/$(TARGET_FRMWRK)/$(RUNTIME)/publish/$(BINNAME) ./$(OBJNAME)
	chmod +x $(OBJNAME)

.PHONY : clean
clean :
	rm -rf bin
	rm -rf obj
	rm -rf $(OBJNAME)
	rm -rf /var/tmp/.net
	rm -rf test.txt
