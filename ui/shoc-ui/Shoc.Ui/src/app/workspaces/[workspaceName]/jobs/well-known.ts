import { IntlMessageId } from "@/i18n/sources";

export const jobTaskTypes: { key: string, display: IntlMessageId }[] = [ 
    {
        key: 'function', 
        display: 'jobs.tasks.types.function'
    },
    {
        key: 'mpi', 
        display: 'jobs.tasks.types.mpi'
    }
]

export const jobTaskTypesMap = Object.assign({}, ...jobTaskTypes.map((entry) => ({[entry.key]: entry.display})));;
