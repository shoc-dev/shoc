import { redirect, RedirectType } from "next/navigation";

export default async function Docs() {

  redirect('/templates', RedirectType.push)
}
