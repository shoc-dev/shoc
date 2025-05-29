import HomeIcon from "@/components/icons/home-icon";
import { SendIcon } from "lucide-react";
import { usePathname } from "next/navigation";
import { useMemo } from "react";
import { useIntl } from "react-intl";

export default function useAppMenu(){

    const intl = useIntl();
    const pathname = usePathname();

    const all: any[] = useMemo(() => [
        { 
            path: `/workspaces`, 
            title: intl.formatMessage({ id: 'workspaces' }),
            icon: HomeIcon,
            visible: true
        },
        { 
            path: `/invitations`, 
            title: intl.formatMessage({ id: 'workspaces.invitations' }),
            icon: SendIcon,
            visible: true
        }
    ], [intl])

    const menu = useMemo(() => all.filter(item => item.visible).map(item => ({
        title: item.title,
        path: item.path,
        icon: item.icon,
        active: item.path === pathname || item.altPaths?.includes(pathname || ''),
        items: item.items?.filter((subItem: any) => subItem.visible).map((subItem: any) => ({
            title: subItem.title,
            path: subItem.path,
            active: subItem.path === pathname
        }))
    })), [all, pathname])


    return menu
}