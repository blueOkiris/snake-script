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
