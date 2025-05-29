import { ReactNode } from "react";
import { SidebarTrigger } from "../ui/sidebar";
import { Separator } from "@radix-ui/react-separator";

export default function BasicHeader( { title,  actions } : { title: string, actions?: ReactNode[] } ) {
    return <div className="flex">
        <SidebarTrigger />
            <Separator orientation="vertical" className="mr-2 !h-4" />
        <div className="flex-1 items-center">
            <h1 className="text-lg truncate font-semibold md:text-xl">{title}</h1>
        </div>
        <div className="flex items-center md:ml-4">
            {actions}
        </div>
    </div>
}