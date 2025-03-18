place = 0;

def create_flag(name, size):
    global place;

    mask = (1 << size) - 1
    smask = mask << place

    mask = str(mask) + "ULL"
    smask = str(smask) + "ULL"

    print("uint64_t "+ name + "() { return (value >> " + str(place) + ") & " + str(mask) + "; }")
    print("void "+ name + "(uint64_t new_value) { value = (value & ~" + str(smask) +") | ( " + str(smask) + " & (new_value << " + str(place)+ ")); }")

    place += size;

create_flag("FIZ", 1)
create_flag("AH", 1)
create_flag("NEP", 1)
create_flag("RES0_0", 5)
create_flag("IOE", 1)
create_flag("DZE", 1)
create_flag("OFE", 1)
create_flag("UFE", 1)
create_flag("IXE", 1)
create_flag("EBF", 1)
create_flag("RES0_1", 1)
create_flag("IDE", 1)
create_flag("Len", 3)
create_flag("FZ16", 1)
create_flag("Stride", 2)
create_flag("RMode", 2)
create_flag("FZ", 1)
create_flag("DN", 1)
create_flag("APH", 1)

place = 0;

print("")

create_flag("IOC", 1)
create_flag("DZC", 1)
create_flag("OFC", 1)
create_flag("UFC", 1)
create_flag("IXC", 1)
create_flag("RES0_0", 2)
create_flag("IDC", 1)
create_flag("RES0_1", 19)
create_flag("QC", 1)
create_flag("V", 1)
create_flag("C", 1)
create_flag("Z", 1)
create_flag("N", 1)