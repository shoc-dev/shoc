import type { Metadata } from 'next'
import getIntl from "@/i18n/get-intl";
import WorkspacesClientPage from './_components/workspaces-client-page';

export const dynamic = 'force-dynamic';

export async function generateMetadata(): Promise<Metadata> {
   
    const intl = await getIntl();

    return {
      title: intl.formatMessage({id: 'workspaces'})
    }
  }

export default async function WorkspacesPage() {
    return <WorkspacesClientPage />
}
