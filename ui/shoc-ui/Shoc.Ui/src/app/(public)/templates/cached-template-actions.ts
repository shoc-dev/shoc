import { rpc } from "@/server-actions/rpc";
import { cache } from "react";

export const getVariant = cache((name: string, variant: string) => {
    return rpc('template/templates/getVariant', { name, variant })
})
